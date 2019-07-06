using System.Collections.Generic;
using System.Linq;
using System;
using EmailReportFunction.Config;
using Microsoft.Extensions.Logging;
using EmailReportFunction.Config.Pipeline;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using EmailReportFunction.Wrappers.Microsoft.EmailTask.EmailReport.Wrappers;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Exceptions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;
using EmailReportFunction.Utils;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.Exceptions;

namespace EmailReportFunction.DataProviders
{
    public class ReleaseDataProvider : IDataProvider
    {
        public ReleaseDataProvider(ReleaseConfiguration pipelineConfiguration,  IReleaseHttpClientWrapper releaseHttpClient, ILogger logger)
        {
            _releaseConfiguration = pipelineConfiguration;
            _releaseHttpClient = releaseHttpClient;
            _logger = logger;
        }

        private Release _release;
        private ReleaseConfiguration _releaseConfiguration;
        private IReleaseHttpClientWrapper _releaseHttpClient;
        private ILogger _logger;

        #region IDataProvider

        public async Task AddReportDataAsync(AbstractReport report)
        {
            if (report is ReleaseReport)
            {
                var releaseReport = report as ReleaseReport;
                using (new PerformanceMeasurementBlock("ReleaseDataProvider", _logger))
                {
                    // TODO - retry
                    _release = await _releaseHttpClient.GetReleaseAsync(_releaseConfiguration.ProjectId, _releaseConfiguration.ReleaseId);
                    if (_release == null)
                    {
                        throw new ReleaseNotFoundException(_releaseConfiguration.ProjectId + ": " + _releaseConfiguration.ReleaseId);
                    }

                    releaseReport.Artifacts = new List<Artifact>(_release.Artifacts);
                    releaseReport.Release = _release;
                    releaseReport.Environment = await GetEnvironmentAsync();
                    releaseReport.Phases = await GetPhasesAsync(releaseReport.Environment);
                    var lastCompletedRelease = await GetReleaseByLastCompletedEnvironmentAsync(releaseReport.Environment);
                    releaseReport.AssociatedChanges = await GetAssociatedChangesAsync(lastCompletedRelease);
                    releaseReport.LastCompletedRelease = lastCompletedRelease;
                    releaseReport.LastCompletedEnvironment = lastCompletedRelease?.Environments?.FirstOrDefault(e => e.DefinitionEnvironmentId == _releaseConfiguration.DefinitionEnvironmentId);
                    releaseReport.CreatedBy = _release.CreatedBy;

                    _logger.LogInformation("ReleaseDataProvider: Fetched release data");
                }
            }
        }

        #endregion

        #region IReleaseDataProvider

        private async Task<List<ChangeData>> GetAssociatedChangesAsync(Release lastCompletedRelease)
        {
            if (lastCompletedRelease == null || (lastCompletedRelease != null && lastCompletedRelease.Id > _releaseConfiguration.ReleaseId))
            {
                // Do not include any changes if already tested with a later version.
                return new List<ChangeData>();
            }

            _logger.LogInformation($"Getting changes between releases {_releaseConfiguration.ReleaseId} & {lastCompletedRelease.Id}");
            return await GetReleaseChanges(lastCompletedRelease.Id);
        }

        private async Task<Release> GetReleaseByLastCompletedEnvironmentAsync(ReleaseEnvironment environment)
        {
            string artifactAlias = null;
            string branchId = null;

            if (_release.Artifacts.Any())
            {
                var primaryArtifact = _release.Artifacts.FirstOrDefault(artifact => artifact.IsPrimary);
                if (primaryArtifact != null)
                {
                    artifactAlias = primaryArtifact.Alias;
                    branchId = primaryArtifact.GetArtifactInfo(Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.ArtifactDefinitionConstants.BranchId)?.Id;
                }
            }

            _logger.LogInformation(
                $"Fetching last release by completed environment id - {_releaseConfiguration.EnvironmentId}" +
                $" with artifact alias - {artifactAlias} & branch id {branchId}");

            // TODO - retry
            var releases = //RetryHelper.Retry(() =>
                await _releaseHttpClient.GetReleasesAsync(_releaseConfiguration.ProjectId, _release.ReleaseDefinitionReference.Id,
                    environment.DefinitionEnvironmentId,
                    (int)EnvironmentStatus.Succeeded | (int)EnvironmentStatus.PartiallySucceeded |
                    (int)EnvironmentStatus.Rejected | (int)EnvironmentStatus.Canceled,
                    ReleaseQueryOrder.Descending, 1, ReleaseExpands.Environments, artifactAlias, branchId)
                           ?? new List<Release>();

            var lastRelease = releases.FirstOrDefault();
            if (lastRelease == null)
            {
                _logger.LogInformation(
                    $"Unable to find any release with completed environment for environment id - {_releaseConfiguration.EnvironmentId}");
            }
            else
            {
                // Fetch more details on the last completed release i.e. AggregatedAnalysis of test outcomes
                // TODO - Retry
                return //RetryHelper.Retry(() => 
                    await _releaseHttpClient.GetReleaseAsync(_releaseConfiguration.ProjectId, lastRelease.Id);
            }

            return lastRelease;
        }

        private async Task<List<PhaseData>> GetPhasesAsync(ReleaseEnvironment environment)
        {
            var phases = new List<PhaseData>();

            int index = 0;
            foreach (var releasePhase in environment.GetPhases())
            {
                var name = "Run on Agent";
                if (environment.DeployPhasesSnapshot.ElementAtOrDefault(index) != null)
                {
                    name = environment.DeployPhasesSnapshot[index].Name;
                }

                var phase = new PhaseData
                {
                    Name = name,
                    Rank = releasePhase.Rank,
                    Status = releasePhase.Status.ToString(),
                    Jobs = releasePhase.DeploymentJobs
                        .Select(job => new JobData
                        {
                            JobStatus = job.Job.Status,
                            JobName = job.Job.Name,
                            Issues = job.Job.Issues.Select(issue => new IssueData
                            {
                                IssueType = issue.IssueType,
                                Message = issue.Message,
                            }).ToList(),
                            Tasks = job
                                .Tasks
                                .Select(item => new TaskData
                                {
                                    AgentName = item.AgentName,
                                    Name = item.Name,
                                    StartTime = item.StartTime,
                                    FinishTime = item.FinishTime,
                                    Status = item.Status,
                                    Issues = item.Issues
                                        .Select(issue => new IssueData
                                        {
                                            IssueType = issue.IssueType,
                                            Message = issue.Message,
                                        })
                                        .ToList(),
                                })
                                .ToList()
                        })
                        .ToList()
                };

                phases.Add(phase);
                index++;
            }

            return await Task.FromResult(phases);
        }

        #endregion

        private async Task<ReleaseEnvironment> GetEnvironmentAsync()
        {
            ReleaseEnvironment environment = _release.Environments.FirstOrDefault(env => env.Id == _releaseConfiguration.EnvironmentId);

            if (_releaseConfiguration.UsePreviousEnvironment)
            {
                if (_release.Environments.IndexOf(environment) - 1 < 0)
                {
                    throw new EmailReportException(
                    $"Unable to find previous environment for given environment id - {_releaseConfiguration.EnvironmentId} in release - {_release.Id}");
                }
                environment = _release.Environments[_release.Environments.IndexOf(environment) - 1];
            }

            if (environment != null)
            {
                return await Task.FromResult(environment);
            }

            throw new EmailReportException(
                $"Unable to find environment with environment id - {_releaseConfiguration.EnvironmentId} in release - {_release.Id}");

        }

        private async Task<List<ChangeData>> GetReleaseChanges(int baseReleaseId)
        {
            _logger.LogInformation($"Fetching changes between releases - {baseReleaseId} & {_releaseConfiguration.ReleaseId}");

            // TODO - Retry
            var releaseChanges = // RetryHelper.Retry(() =>
                await _releaseHttpClient.GetReleaseChangesAsync(_releaseConfiguration.ProjectId, _releaseConfiguration.ReleaseId, baseReleaseId) ?? new List<Change>();

            if (!releaseChanges.Any())
            {
                _logger.LogInformation($"No changes found between releases - {baseReleaseId} & {_releaseConfiguration.ReleaseId}");
            }

            return releaseChanges
                .Select(item => new ChangeData
                {
                    Id = item.Id,
                    Author = item.Author,
                    Location = item.Location,
                    Message = item.Message,
                    Timestamp = item.Timestamp
                }).ToList();
        }

    }
}