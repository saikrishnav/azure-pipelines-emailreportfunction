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
    public class TestResultDetailsParserForRun : AbstractTestResultDetailsParser
    {
        public TestResultDetailsParserForRun(TestResultsDetails testResultDetailsForRunGroup, ILogger logger)
            : base(testResultDetailsForRunGroup, logger)
        {
            if (!string.Equals(testResultDetailsForRunGroup.GroupByField,
                TestResultsConstants.TestRun, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new EmailReportException(
                    $"Expected test result group type to be {TestResultsConstants.TestRun}. But found {testResultDetailsForRunGroup.GroupByField}");
            }
        }

        public override List<TestSummaryItem> GetSummaryItems()
        {
            return TestResultDetails.ResultsForGroup.Select(GetTestRunSummaryInfo).ToList();
        }

        public override string GetGroupByValue(TestResultsDetailsForGroup group)
        {
            return ReadGroupByValue(group).Name;
        }

        /// <summary>
        ///     Returns a summary item for a given tcm data object for a test run.
        /// </summary>
        private TestSummaryItem GetTestRunSummaryInfo(TestResultsDetailsForGroup resultsForGroup)
        {
            _logger.LogInformation($"Getting Test summary data for test run - {resultsForGroup.GroupByValue}");

            TestRunInfo runInfo = ReadGroupByValue(resultsForGroup);

            var summaryItem = new TestSummaryItem
            {
                Name = string.IsNullOrWhiteSpace(runInfo.Name)
                    ? runInfo.Id.ToString()
                    : runInfo.Name,
                Id = runInfo.Id.ToString()
            };

            ParseBaseData(resultsForGroup, summaryItem);

            return summaryItem;
        }

        private static TestRunInfo ReadGroupByValue(TestResultsDetailsForGroup resultsForGroup)
        {
            var jobj = resultsForGroup.GroupByValue as JObject;

            if (jobj == null)
            {
                throw new InvalidTestResultDataException("Unable to parse json value for GroupByValue");
            }

            var serializableRunInfo = jobj.ToObject<TestRunInfo>();

            if (serializableRunInfo == null || serializableRunInfo.Id <= 0)
            {
                throw new InvalidTestResultDataException($"Invalid run id detected in json - {serializableRunInfo?.Id}");
            }
            return serializableRunInfo;
        }

        #region Helper Class

        public class TestRunInfo
        {
            [DataMember(Name = "id")]
            public int Id { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }
        }
        #endregion
    }
}
