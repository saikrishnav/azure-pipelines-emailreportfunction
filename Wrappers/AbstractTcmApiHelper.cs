using EmailReportFunction.Config;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.Exceptions;
using EmailReportFunction.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace EmailReportFunction.Wrappers
{
    public abstract class AbstractTcmApiHelper : ITcmApiHelper
    {
        internal const int MaxItemsSupported = 100;

        protected ITestManagementHttpClientWrapper _tcmClient;
        protected EmailReportConfiguration _emailReportConfig;
        protected ILogger _logger;

        public AbstractTcmApiHelper(ITestManagementHttpClientWrapper tcmClient, EmailReportConfiguration emailReportConfiguration, ILogger logger)
        {
            _logger = logger;
            _tcmClient = tcmClient;
            _emailReportConfig = emailReportConfiguration;
        }

        public async Task<List<IdentityRef>> GetTestResultOwnersAsync(IList<TestCaseResult> resultIds)
        {
            using (new PerformanceMeasurementBlock("Fetching Test result owners", _logger))
            {
                var fieldsToFetch = new List<string>
                {
                    FieldNameConstants.Owner
                };

                List<List<TestCaseResult>> chunks = resultIds.Split(MaxItemsSupported);

                IList<TestCaseResult>[] chunkResults = await chunks.ParallelSelectAsync(async chunk =>
                {
                    // TODO - retry
                    TestResultsQuery resultQuery = // RetryHelper.Retry(() =>
                      await _tcmClient.GetTestResultsByQueryAsync(_emailReportConfig.PipelineConfiguration.ProjectId, new TestResultsQuery
                      {
                          Fields = fieldsToFetch,
                          Results = chunk
                      });
                    return resultQuery.Results;
                });

                return chunkResults.Merge()
                    .Where(tcr => IdentityRefHelper.IsValid(tcr.Owner))
                    .Select(tcr => tcr.Owner)
                    .DistinctBy(identity => IdentityRefHelper.GetUniqueName(identity).ToLowerInvariant())
                    .ToList();
            }
        }

        public async Task<TestCaseResult> GetTestResultByIdAsync(int runId, int testCaseResultId,
            CancellationToken cancellationToken = new CancellationToken())
        {
            //TODO - retry
            return //RetryHelper.Retry(() =>
               await _tcmClient.GetTestResultByIdAsync(_emailReportConfig.PipelineConfiguration.ProjectId, runId, testCaseResultId, cancellationToken: cancellationToken);
        }

        public async Task<List<WorkItemReference>> QueryTestResultBugsAsync(string automatedTestName, int testCaseId, 
            CancellationToken cancellationToken = new CancellationToken())
        {
            return // TODO - RetryHelper.Retry(() =>
                await _tcmClient.QueryTestResultWorkItemsAsync(_emailReportConfig.PipelineConfiguration.ProjectId, automatedTestName, testCaseId, "Microsoft.BugCategory", cancellationToken);
        }

        public async Task<List<TestSummaryItem>> GetTestRunSummaryWithPriorityAsync()
        {
            using (new PerformanceMeasurementBlock("Get Test Run Summary from TCM", _logger))
            {
                TestResultDetailsParserForRun summaryByRun = null;
                IReadOnlyDictionary<TestOutcomeForPriority, TestResultDetailsParserForPriority>
                    testResultDetailsByOutcomeForPriorityGroup = null;

                var runSummaryFetchTask = Task.Run(async () =>
                {
                    using (new PerformanceMeasurementBlock("Get test summary data by test run", _logger))
                    {
                        var result = await GetTestResultsDetailsAsync();
                        summaryByRun = new TestResultDetailsParserForRun(result, _logger);
                    }
                });

                var prioritySummaryFetchTask = Task.Run(async () =>
                {
                    using (new PerformanceMeasurementBlock("Get test summary data by priority", _logger))
                    {
                        testResultDetailsByOutcomeForPriorityGroup = await GetTestSummaryDataByPriorityAsync();
                    }
                });

                await Task.WhenAll(runSummaryFetchTask, prioritySummaryFetchTask);

                if (summaryByRun == null || testResultDetailsByOutcomeForPriorityGroup == null)
                {
                    throw new EmailReportException("unable to fetch summary data from tcm");
                }

                _logger.LogInformation("parsing data to get Test run ids");

                return GetSummaryByRun(summaryByRun,
                    testResultDetailsByOutcomeForPriorityGroup);
            }
        }

        protected abstract Task<TestResultsDetails> GetTestResultsDetailsAsync();

        protected abstract Task<TestResultsDetails> GetTestResultsDetailsAsync(IEnumerable<TestOutcome> testOutcomes);

        public abstract Task<TestResultSummary> GetTestResultSummaryAsync();

        public abstract Task<TestResultsDetails> GetTestSummaryAsync(string groupBy, params TestOutcome[] includeOutcomes);

        public abstract Task<TestResultsDetails> GetTestSummaryAsync(PipelineConfiguration pipelineConfiguration, string groupBy, params TestOutcome[] includeOutcomes);

        public abstract Task<TestResultSummary> QueryTestResultsReportAsync(PipelineConfiguration releaseConfig = null);

        public abstract Task<TestResultsQuery> GetTestResultsByQueryAsync(TestResultsQuery query);

        #region Helper methods

        private async Task<IReadOnlyDictionary<TestOutcomeForPriority, TestResultDetailsParserForPriority>> GetTestSummaryDataByPriorityAsync()
        {
            var outcomeFilters = new Dictionary
                <TestOutcomeForPriority, TestOutcome[]>
            {
                {TestOutcomeForPriority.Passed, new[] {TestOutcome.Passed}},
                {TestOutcomeForPriority.Failed, new[] {TestOutcome.Failed}},
                {TestOutcomeForPriority.Inconclusive, new[] {TestOutcome.Inconclusive}},
                {TestOutcomeForPriority.NotExecuted, new[] {TestOutcome.NotExecuted}},
                {
                    TestOutcomeForPriority.Other,
                    EnumHelper.GetEnumsExcept(TestOutcome.Failed, TestOutcome.Passed, TestOutcome.Inconclusive,
                        TestOutcome.NotExecuted)
                }
            };

            var testResultDetailsForOutcomes =
                new ConcurrentDictionary<TestOutcomeForPriority, TestResultDetailsParserForPriority>();

            var analyzerWorker = new ActionBlock<TestOutcomeForPriority>(async supportedOutcome =>
            {
                _logger.LogInformation(
                    $"Fetching test summary data by priority for supported outcome type - {supportedOutcome}");

                var result = await this.GetTestResultsDetailsAsync(outcomeFilters[supportedOutcome]);

                _logger.LogInformation(
                    $"Fetched test summary data by priority for supported outcome type - {supportedOutcome}");

                testResultDetailsForOutcomes.TryAdd(supportedOutcome, new TestResultDetailsParserForPriority(result, _logger));
            });

            await PostAllAndComplete(analyzerWorker, outcomeFilters.Keys);
            return new ReadOnlyDictionary<TestOutcomeForPriority, TestResultDetailsParserForPriority>(testResultDetailsForOutcomes);
        }

        protected static string GetOutcomeFilter(IEnumerable<TestOutcome> outcomes)
        {
            var filter = outcomes.Any()
                ? $"Outcome eq {string.Join(",", outcomes.Select(outcome => (int)outcome).Distinct())}"
                : null;
            return filter;
        }

        /// <summary>
        ///     Merges the tcm summary data grouped by test run, & tcm summary data grouped by priority to create summary object
        /// </summary>
        /// <returns>Returns a Dictionary of Summary item grouped by test run</returns>
        private List<TestSummaryItem> GetSummaryByRun(
            TestResultDetailsParserForRun testResultByRun,
            IReadOnlyDictionary<TestOutcomeForPriority, TestResultDetailsParserForPriority> testResultsForPriorityByOutcome)
        {
            var summaryItemByRun = testResultByRun.GetSummaryItems();

            foreach (var summaryItem in summaryItemByRun)
            {
                var totalCountForTestOutcomeByPriority = new Dictionary<int, Dictionary<TestOutcomeForPriority, int>>();
                summaryItem.TestCountForOutcomeByPriority = totalCountForTestOutcomeByPriority;

                foreach (TestOutcomeForPriority supportedTestOutcome in testResultsForPriorityByOutcome.Keys)
                {
                    IReadOnlyDictionary<int, int> resultCountByPriority =
                        testResultsForPriorityByOutcome[supportedTestOutcome].GetTestResultsForRun(
                            int.Parse(summaryItem.Id));

                    foreach (var priority in resultCountByPriority.Keys)
                    {
                        if (!totalCountForTestOutcomeByPriority.ContainsKey(priority))
                        {
                            totalCountForTestOutcomeByPriority[priority] = new Dictionary<TestOutcomeForPriority, int>();
                        }

                        Dictionary<TestOutcomeForPriority, int> testCountByOutcome = totalCountForTestOutcomeByPriority[priority];

                        if (!testCountByOutcome.ContainsKey(supportedTestOutcome))
                        {
                            testCountByOutcome[supportedTestOutcome] = 0;
                        }

                        testCountByOutcome[supportedTestOutcome] += resultCountByPriority[priority];
                    }
                }
            }

            return summaryItemByRun;
        }

        public Task PostAllAndComplete<T>(ITargetBlock<T> actionBlock, IEnumerable<T> inputs)
        {
            foreach (T input in inputs)
            {
                actionBlock.Post(input);
            }

            actionBlock.Complete();
            return actionBlock.Completion;
        }

        #endregion
    }
}
