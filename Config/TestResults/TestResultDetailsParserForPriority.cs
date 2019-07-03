using EmailReportFunction.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EmailReportFunction.Config.TestResults
{
    public class TestResultDetailsParserForPriority : AbstractTestResultDetailsParser
    {
        public TestResultDetailsParserForPriority(TestResultsDetails testResultDetailsForPriorityGroup, ILogger logger)
            : base(testResultDetailsForPriorityGroup, logger)
        {
            if (!string.Equals(testResultDetailsForPriorityGroup.GroupByField,
                TestResultsConstants.Priority, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new EmailReportException(
                    $"Expected test result group type to be {TestResultsConstants.Priority}. But found {testResultDetailsForPriorityGroup.GroupByField}");
            }
        }

        public override string GetGroupByValue(TestResultsDetailsForGroup group)
        {
            return GetPriority(group.GroupByValue).ToString();
        }

        public override List<TestSummaryItem> GetSummaryItems()
        {
            return TestResultDetails.ResultsForGroup.Select(group =>
            {
                var priority = GetPriority(group.GroupByValue);
                var summaryItemDto = new TestSummaryItem
                {
                    Name = priority.ToString(),
                    Id = priority.ToString()
                };

                ParseBaseData(group, summaryItemDto);
                return summaryItemDto;
            }).ToList();
        }

        public IReadOnlyDictionary<int, int> GetTestResultsForRun(int runId)
        {
            Dictionary<int, Dictionary<int, int>> testResultsByPriority = GetTestCountByPriorityInTestRun();

            return testResultsByPriority.ContainsKey(runId)
                ? testResultsByPriority[runId]
                : new Dictionary<int, int>();
        }

        private static int GetPriority(object groupByValue)
        {
            int priority;
            if (!int.TryParse(groupByValue as string, out priority))
            {
                throw new InvalidTestResultDataException(
                    $"Expected priority value to be integer in {groupByValue}");
            }
            return priority;
        }

        /// <summary>
        ///     Converts tcm summary data grouped by priority to a dictionary of test counts by priority grouped by test run
        /// </summary>
        /// <returns>
        ///     Returns a dictionary of test counts by priority grouped by Test run => Dictionary of {test runId, Dictionary
        ///     of [priority , test count]}
        /// </returns>
        private Dictionary<int, Dictionary<int, int>> GetTestCountByPriorityInTestRun()
        {
            var testResultsByPriority = new Dictionary<int, Dictionary<int, int>>();

            foreach (TestResultsDetailsForGroup testResultsByGroup in TestResultDetails.ResultsForGroup)
            {
                var priority = GetPriority(testResultsByGroup.GroupByValue);

                foreach (TestCaseResult result in testResultsByGroup.Results)
                {
                    int testRunId;

                    if (result.TestRun == null)
                    {
                        throw new InvalidTestResultDataException(
                            $"Test run field is null in Test result object with test id - {result.Id}");
                    }

                    if (!int.TryParse(result.TestRun?.Id, out testRunId))
                    {
                        throw new InvalidTestResultDataException(
                            $"Unable to parse test run id to integer in {result.TestRun?.Id}");
                    }

                    if (!testResultsByPriority.ContainsKey(testRunId))
                    {
                        testResultsByPriority[testRunId] = new Dictionary<int, int>();
                    }

                    Dictionary<int, int> resultsByPriorityForRun = testResultsByPriority[testRunId];

                    var testCountByPriority = resultsByPriorityForRun.ContainsKey(priority)
                        ? resultsByPriorityForRun[priority]
                        : 0;

                    resultsByPriorityForRun[priority] = testCountByPriority + 1;
                }
            }

            return testResultsByPriority;
        }
    }
}
