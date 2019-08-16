using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmailReportFunction.Wrappers
{
    /// <summary>
    ///     This class is used for making testing easier.
    ///     Preview apis are implemented in <see cref="TestManagementHttpClient" /> without marking them virtual.
    /// </summary>
    public class TestManagementHttpClientWrapper : ITestManagementHttpClientWrapper
    {
        private readonly TestManagementHttpClient _tcm;

        public TestManagementHttpClientWrapper(TestManagementHttpClient tcm)
        {
            _tcm = tcm ?? throw new ArgumentNullException(nameof(tcm));
        }

        public static TestManagementHttpClientWrapper CreateInstance(VssCredentials vssCredentials, string uri)
        {
            var tcm = new TestManagementHttpClient(new Uri(uri), vssCredentials);
            return new TestManagementHttpClientWrapper(tcm);
        }

        public Task<TestResultsDetails> GetTestResultDetailsForReleaseAsync(string project, int releaseId,
            int releaseEnvId, string publishContext = null,
            string groupBy = null, string filter = null, object userState = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _tcm.GetTestResultDetailsForReleaseAsync(
                project: project,
                releaseId: releaseId,
                releaseEnvId: releaseEnvId,
                publishContext: publishContext,
                groupBy: groupBy,
                filter: filter,
                @orderby: null,
                shouldIncludeResults: null,
                userState: userState,
                cancellationToken: cancellationToken);
        }

        public Task<TestResultsDetails> GetTestResultDetailsForBuildAsync(string project, int buildId,
            string publishContext = null,
            string groupBy = null, string filter = null, object userState = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _tcm.GetTestResultDetailsForBuildAsync(
                project: project,
                buildId: buildId,
                publishContext: publishContext,
                groupBy: groupBy,
                filter: filter,
                @orderby: null,
                shouldIncludeResults: null,
                userState: userState,
                cancellationToken: cancellationToken);
        }

        public Task<TestResultsQuery> GetTestResultsByQueryAsync(string project, TestResultsQuery query)
        {
            return _tcm.GetTestResultsByQueryAsync(query, project);
        }

        public Task<TestCaseResult> GetTestResultByIdAsync(
            string project,
            int runId,
            int testCaseResultId,
            ResultDetails? detailsToInclude = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _tcm.GetTestResultByIdAsync(project, runId, testCaseResultId, detailsToInclude, cancellationToken: cancellationToken);
        }

        public Task<List<WorkItemReference>> QueryTestResultWorkItemsAsync(string project,
            string automatedTestName,
            int testCaseId, string workItemCategory,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _tcm.QueryTestResultWorkItemsAsync(project, workItemCategory, automatedTestName, testCaseId,
                cancellationToken: cancellationToken);
        }

        public Task<TestResultSummary> QueryTestResultsReportForReleaseAsync(string project,
            int releaseId,
            int releaseEnvId,
            string publishContext = null,
            bool? includeFailureDetails = null,
            ReleaseReference releaseToCompare = null,
            object userState = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _tcm.QueryTestResultsReportForReleaseAsync(project, releaseId, releaseEnvId, publishContext, includeFailureDetails, releaseToCompare, userState, cancellationToken);
        }

        public Task<TestResultSummary> QueryTestResultsReportForBuildAsync(string project,
            int buildId,
            string publishContext = null,
            bool? includeFailureDetails = null,
            BuildReference buildToCompare = null,
            object userState = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _tcm.QueryTestResultsReportForBuildAsync(project, buildId, publishContext, includeFailureDetails, buildToCompare, userState, cancellationToken);
        }
    }
}
