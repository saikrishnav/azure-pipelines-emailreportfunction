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

        public async override Task<TestResultSummary> QueryTestResultsReportAsync()
        {
            return //TODO - RetryHelper.Retry(()
                await _tcmClient.QueryTestResultsReportForReleaseAsync(
                    _releaseConfig.ProjectName,
                    _releaseConfig.ReleaseId,
                    _releaseConfig.EnvironmentId);
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
            using (new PerformanceMeasurementBlock($"Fetching test summary for groupby - {groupBy}  & outomes - {string.Join(",", includeOutcomes)}", _logger))
            {
                return //TODO - RetryHelper.Retry(() =>
                    await _tcmClient.GetTestResultDetailsForReleaseAsync(
                        _releaseConfig.ProjectId,
                        _releaseConfig.ReleaseId,
                        _releaseConfig.EnvironmentId,
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
    }
}
