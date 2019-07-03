using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EmailReportFunction.Config;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace EmailReportFunction.Wrappers
{
    public class ReleaseTcmApiHelper : AbstractTcmApiHelper
    {
        private ReleaseConfiguration _releaseConfig;

        public ReleaseTcmApiHelper(ITestManagementHttpClientWrapper tcmClient, EmailReportConfiguration emailReportConfiguration, ILogger logger) 
            : base(tcmClient, emailReportConfiguration, logger)
        {
            _releaseConfig = _emailReportConfig.PipelineConfiguration as ReleaseConfiguration;
            if (_releaseConfig == null)
            {
                throw new NotSupportedException();
            }
        }

        public async override Task<TestResultSummary> QueryTestResultsReportAsync(PipelineConfiguration releaseConfig = null)
        {
            var releaseConfiguration = (releaseConfig != null && releaseConfig is ReleaseConfiguration) 
                ? (releaseConfig as ReleaseConfiguration) : _releaseConfig;
            return //TODO - RetryHelper.Retry(()
                await _tcmClient.QueryTestResultsReportForReleaseAsync(
                    releaseConfiguration.ProjectName,
                    releaseConfiguration.ReleaseId,
                    releaseConfiguration.EnvironmentId);
        }

        public async override Task<TestResultSummary> GetTestResultSummaryAsync()
        {
            using (new PerformanceMeasurementBlock("Get Test Run Summary from TCM", _logger))
            {
                return await _tcmClient.QueryTestResultsReportForReleaseAsync(
                    _releaseConfig.ProjectName,
                    _releaseConfig.ReleaseId,
                    _releaseConfig.EnvironmentId,
                    includeFailureDetails: true);
            }
        }

        public async override Task<TestResultsDetails> GetTestSummaryAsync(string groupBy, params TestOutcome[] includeOutcomes)
        {
           return await GetTestSummaryAsync(_releaseConfig, groupBy, includeOutcomes);
        }

        public async override Task<TestResultsDetails> GetTestSummaryAsync(PipelineConfiguration pipelineConfiguration, string groupBy, params TestOutcome[] includeOutcomes)
        {
            var releaseConfiguration = (pipelineConfiguration != null && pipelineConfiguration is ReleaseConfiguration)
                    ? (pipelineConfiguration as ReleaseConfiguration) : _releaseConfig;
            using (new PerformanceMeasurementBlock($"Fetching test summary for groupby - {groupBy}  & outomes - {string.Join(",", includeOutcomes)}", _logger))
            {
                return //TODO - RetryHelper.Retry(() =>
                    await _tcmClient.GetTestResultDetailsForReleaseAsync(
                        releaseConfiguration.ProjectId,
                        releaseConfiguration.ReleaseId,
                        releaseConfiguration.EnvironmentId,
                        SourceWorkflow.ContinuousDelivery,
                        groupBy,
                        GetOutcomeFilter(includeOutcomes));
            }
        }

        protected async override Task<TestResultsDetails> GetTestResultsDetailsAsync()
        {
            return // TODO - RetryHelper.Retry(() =>
                await _tcmClient.GetTestResultDetailsForReleaseAsync(
                    _releaseConfig.ProjectId,
                    _releaseConfig.ReleaseId,
                    _releaseConfig.EnvironmentId,
                    SourceWorkflow.ContinuousDelivery,
                    TestResultsConstants.TestRun);
        }

        protected async override Task<TestResultsDetails> GetTestResultsDetailsAsync(IEnumerable<TestOutcome> testOutcomes)
        {
            var filter = GetOutcomeFilter(testOutcomes);

            return // TODO - RetryHelper.Retry(() =>
                await _tcmClient.GetTestResultDetailsForReleaseAsync(
                    _releaseConfig.ProjectId, 
                    _releaseConfig.ReleaseId,
                    _releaseConfig.EnvironmentId, 
                    SourceWorkflow.ContinuousDelivery, 
                    TestResultsConstants.Priority,
                    filter);
        }

        public override async Task<TestResultsQuery> GetTestResultsByQueryAsync(TestResultsQuery query)
        {
            return // TODO - RetryHelper.Retry(() =>
                 await _tcmClient.GetTestResultsByQueryAsync(_emailReportConfig.PipelineConfiguration.ProjectName, query);
        }
    }
}
