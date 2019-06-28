using Microsoft.TeamFoundation.TestManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmailReportFunction.Wrappers
{
    /// <summary>
    ///     This interface is used for making testing easier.
    ///     Preview apis are implemented in <see cref="TestManagementHttpClient" />  without marking them virtual.
    /// </summary>
    public interface ITestManagementHttpClientWrapper
    {
        Task<TestResultsDetails> GetTestResultDetailsForReleaseAsync(
            string project,
            int releaseId,
            int releaseEnvId,
            string publishContext = null,
            string groupBy = null,
            string filter = null,
            object userState = null,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<TestResultsDetails> GetTestResultDetailsForBuildAsync(
            string project,
            int buildId,
            string publishContext = null,
            string groupBy = null,
            string filter = null,
            object userState = null,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<List<WorkItemReference>> QueryTestResultWorkItemsAsync(string project,
            string automatedTestName,
            int testCaseId, string workItemCategory,
            CancellationToken cancellationToken = new CancellationToken());

        Task<TestCaseResult> GetTestResultByIdAsync(
            string project,
            int runId,
            int testCaseResultId,
            ResultDetails? detailsToInclude = null,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<TestResultsQuery> GetTestResultsByQueryAsync(string project, TestResultsQuery query);

        Task<TestResultSummary> QueryTestResultsReportForReleaseAsync(
            string project,
            int releaseId,
            int releaseEnvId,
            string publishContext = null,
            bool? includeFailureDetails = null,
            ReleaseReference releaseToCompare = null,
            object userState = null,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<TestResultSummary> QueryTestResultsReportForBuildAsync(
            string project,
            int buildId,
            string publishContext = null,
            bool? includeFailureDetails = null,
            BuildReference buildToCompare = null,
            object userState = null,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
