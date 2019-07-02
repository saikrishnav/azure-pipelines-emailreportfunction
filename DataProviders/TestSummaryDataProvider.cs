using EmailReportFunction.Config;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.Utils;
using EmailReportFunction.Wrappers;
using Microsoft.Build.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.DataProviders
{
    public class TestSummaryData
    {
        public List<TestSummaryGroup> TestSummaryGroups { get; set; }
        public TestResultSummary ResultSummary { get; set; }
    }

    public class TestSummaryDataProvider : IDataProvider<TestSummaryData>
    {
        private readonly ITcmApiHelper _tcmApiHelper;
        private readonly EmailReportConfiguration _emailReportConfiguration;
        private readonly ILogger _logger;

        public TestSummaryDataProvider(ITcmApiHelper tcmApiHelper, EmailReportConfiguration emailReportConfiguration, ILogger logger)
        {
            _tcmApiHelper = tcmApiHelper;
            _emailReportConfiguration = emailReportConfiguration;
            _logger = logger;
        }

        public async Task<TestSummaryData> GetDataAsync()
        {
            using (new PerformanceMeasurementBlock(nameof(TestSummaryDataProvider), _logger))
            {                
                var priorityGroup = await GetTestRunSummaryWithPriority();
                var testSummaryGroups = new List<TestSummaryGroup>() { priorityGroup };
                var summary =  //TODO - RetryHelper.Retry(() => 
                    await _tcmApiHelper.GetTestResultSummaryAsync();

                if (_emailReportConfiguration.GroupTestSummaryBy.Contains(TestResultsGroupingType.Priority))
                {
                    var prioritySummary = await GetTestSummaryByPriorityAsync();
                    testSummaryGroups.Add(prioritySummary);
                }
                _logger.LogInformation("Fetched data for test summary");
                return new TestSummaryData()
                {
                    ResultSummary = summary,
                    TestSummaryGroups = testSummaryGroups
                };
            }
        }

        private async Task<TestSummaryGroup> GetTestRunSummaryWithPriority()
        {
            var testSummaryItemsByRuns = await _tcmApiHelper.GetTestRunSummaryWithPriorityAsync();

            var testSummaryByRun = new TestSummaryGroup
            {
                GroupingType = TestResultsGroupingType.Run,
                Runs = testSummaryItemsByRuns
            };
            return testSummaryByRun;
        }

        private async Task<TestSummaryGroup> GetTestSummaryByPriorityAsync()
        {
            TestResultsDetails testSummaryItemsByRuns = await _tcmApiHelper.GetTestSummaryAsync(TestResultsConstants.Priority);

            var testResultDetailsParserForPriority = new TestResultDetailsParserForPriority(testSummaryItemsByRuns, _logger);
            var testSummaryByRun = new TestSummaryGroup
            {
                GroupingType = TestResultsGroupingType.Priority,
                Runs = testResultDetailsParserForPriority.GetSummaryItems()
            };
            return testSummaryByRun;
        }
    }
}
