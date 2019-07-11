using EmailReportFunction.Config;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.Utils;
using EmailReportFunction.Wrappers;
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
    public class TestSummaryDataProvider : IDataProvider
    {
        private readonly ITcmApiHelper _tcmApiHelper;
        private readonly ReportDataConfiguration _reportDataConfiguration;
        private readonly ILogger _logger;

        public TestSummaryDataProvider(ITcmApiHelper tcmApiHelper, ReportDataConfiguration reportDataConfiguration, ILogger logger)
        {
            _tcmApiHelper = tcmApiHelper;
            _reportDataConfiguration = reportDataConfiguration;
            _logger = logger;
        }

        public async Task AddReportDataAsync(AbstractReport reportData)
        {
            using (new PerformanceMeasurementBlock(nameof(TestSummaryDataProvider), _logger))
            {                
                var priorityGroup = await GetTestRunSummaryWithPriority();
                var testSummaryGroups = new List<TestSummaryGroup>() { priorityGroup };
                var summary =  //TODO - RetryHelper.Retry(() => 
                    await _tcmApiHelper.GetTestResultSummaryAsync();

                if (_reportDataConfiguration.GroupTestSummaryBy == TestResultsGroupingType.Priority)
                {
                    var prioritySummary = await GetTestSummaryByPriorityAsync();
                    testSummaryGroups.Add(prioritySummary);
                }

                _logger.LogInformation("Fetched data for test summary");
                reportData.Summary = summary;
                reportData.TestSummaryGroups = testSummaryGroups;
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
