using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.TestResults
{
    public abstract class AbstractTestResultDetailsParser : ITestResultDetailsParser
    {
        protected readonly TestResultsDetails TestResultDetails;
        protected readonly ILogger _logger;

        protected AbstractTestResultDetailsParser(TestResultsDetails testResultDetails, ILogger logger)
        {
            TestResultDetails = testResultDetails;
            _logger = logger;
        }

        public abstract List<TestSummaryItem> GetSummaryItems();

        public abstract string GetGroupByValue(TestResultsDetailsForGroup group);

        /// <summary>
        /// Get Duration, TotalTests & test count by outcome
        /// Calculating total duration, as the tcm data has duration by test outcome only.
        /// </summary>
        protected void ParseBaseData(TestResultsDetailsForGroup resultsForGroup, TestSummaryItem summaryItem)
        {
            summaryItem.TotalTests = resultsForGroup.Results.Count;
            summaryItem.TestCountByOutCome = new Dictionary<TestOutcome, int>();

            foreach (var aggregatedResultsByOutcome in resultsForGroup.ResultsCountByOutcome)
            {
                summaryItem.TestCountByOutCome[aggregatedResultsByOutcome.Key] =
                    aggregatedResultsByOutcome.Value.Count;
                summaryItem.Duration += aggregatedResultsByOutcome.Value.Duration;
            }
        }

    }
}
