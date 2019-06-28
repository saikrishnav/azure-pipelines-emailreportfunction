using EmailReportFunction.Config.TestResults;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmailReportFunction.Wrappers
{
    public interface ITcmApiHelper
    {
        Task<List<IdentityRef>> GetTestResultOwnersAsync(IList<TestCaseResult> resultIds);

        Task<TestCaseResult> GetTestResultByIdAsync(
            int runId,
            int testCaseResultId,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<List<WorkItemReference>> QueryTestResultBugsAsync(string automatedTestName, int testCaseId,
            CancellationToken cancellationToken = new CancellationToken());

        Task<List<TestSummaryItem>> GetTestRunSummaryWithPriorityAsync();

        Task<TestResultsDetails> GetTestSummaryAsync(string groupBy, params TestOutcome[] includeOutcomes);

        Task<TestResultSummary> GetTestResultSummaryAsync();

        Task<TestResultSummary> QueryTestResultsReportAsync();
    }
}
