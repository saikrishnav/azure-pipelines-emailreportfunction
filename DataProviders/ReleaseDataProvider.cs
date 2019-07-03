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

namespace EmailReportFunction.DataProviders
{
    public class ReleaseDataProvider : IReleaseDataProvider
    {
        public ReleaseDataProvider(ReleaseConfiguration pipelineConfiguration, 
            IReleaseHttpClientWrapper releaseHttpClient, 
            IDataProvider<List<IdentityRef>> failedTestOwnersDataProvider,
            IDataProvider<FilteredTestResultData> testResultsDataProvider,
            IDataProvider<TestSummaryData> testSummaryDataProvider,
            ILogger logger)
        {
            _failedTestOwnersDataProvider = failedTestOwnersDataProvider;
            _testResultsDataProvider = testResultsDataProvider;
            _releaseConfiguration = pipelineConfiguration;
            _releaseHttpClient = releaseHttpClient;
            _testSummaryDataProvider = testSummaryDataProvider;
            _logger = logger;
        }

        private ReleaseConfiguration _releaseConfiguration;
        private IReleaseHttpClientWrapper _releaseHttpClient;
        private IDataProvider<List<IdentityRef>> _failedTestOwnersDataProvider;
        private IDataProvider<FilteredTestResultData> _testResultsDataProvider;
        private IDataProvider<TestSummaryData> _testSummaryDataProvider;
        private ILogger _logger;


        #region IDataProvider

        public async Task<IPipelineData> GetDataAsync()
        {
            using (new PerformanceMeasurementBlock("ReleaseDataProvider", _logger))
            {
                // TODO - retry
                Release release = await _releaseHttpClient.GetReleaseAsync(_releaseConfiguration.ProjectId, _releaseConfiguration.ReleaseId);
                if (release == null)
                {
                    throw new ReleaseNotFoundException(_releaseConfiguration.ProjectId + ": " + _releaseConfiguration.ReleaseId);
                }

                release.Properties.Add(ReleaseData.ReleaseEnvironmentIdString, _releaseConfiguration.EnvironmentId);
                release.Properties.Add(ReleaseData.UsePrevReleaseEnvironmentString, _releaseConfiguration.UsePreviousEnvironment);
                var releaseData = new ReleaseData(release, this, _failedTestOwnersDataProvider, _testResultsDataProvider, _testSummaryDataProvider);

                _logger.LogInformation("ReleaseDataProvider: Fetched release data");
                return releaseData;
            }
        }

        #endregion

        #region IReleaseDataProvider

        public async Task<List<ChangeData>> GetAssociatedChanges(Release lastCompletedRelease)
        {
            if (lastCompletedRelease == null || (lastCompletedRelease != null && lastCompletedRelease.Id > _releaseConfiguration.ReleaseId))
            {
                // Do not include any changes if already tested with a later version.
                return new List<ChangeData>();
            }

            _logger.LogInformation($"Getting changes between releases {_releaseConfiguration.ReleaseId} & {lastCompletedRelease.Id}");
            return await GetReleaseChanges(lastCompletedRelease.Id);
        }

        public async Task<Release> GetReleaseByLastCompletedEnvironment(Release release, ReleaseEnvironment environment)
        {
            string artifactAlias = null;
            string branchId = null;

            if (release.Artifacts.Any())
            {
                var primaryArtifact = release.Artifacts.FirstOrDefault(artifact => artifact.IsPrimary);
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
                await _releaseHttpClient.GetReleasesAsync(_releaseConfiguration.ProjectId, release.ReleaseDefinitionReference.Id,
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

        public async Task<List<PhaseData>> GetPhases(ReleaseEnvironment environment)
        {
            var phases = new List<PhaseData>();
            var releaseDeployPhases = environment.GetPhases();

            int index = 0;
            foreach (var releasePhase in releaseDeployPhases)
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