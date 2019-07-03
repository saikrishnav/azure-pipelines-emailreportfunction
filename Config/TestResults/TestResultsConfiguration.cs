using EmailReportFunction.Utils;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.TestResults
{
    public class TestResultsConfiguration
    {
        public bool IncludeFailedTests { get; set; }

        public bool IncludePassedTests { get; set; }

        public bool IncludeInconclusiveTests { get; set; }

        public bool IncludeNotExecutedTests { get; set; }

        public bool IncludeOtherTests { get; set; }

        public TestResultsGroupingType GroupingType { get; set; }

        public int MaxItemsToShow { get; set; }

        public TestResultsGroupingType[] GroupTestSummaryBy { get; set; }

        public bool IncludeOthersInTotalCount { get; set; }

        public IEnumerable<TestOutcome> GetOutcomesforFailedList()
        {
            List<TestOutcome> failureOutcomes = new List<TestOutcome>();
            if (IncludeFailedTests)
            {
                failureOutcomes.Add(TestOutcome.Failed);
            }

            if (IncludeNotExecutedTests)
            {
                failureOutcomes.Add(TestOutcome.NotExecuted);
            }

            if (IncludeInconclusiveTests)
            {
                failureOutcomes.Add(TestOutcome.Inconclusive);
            }
            if (IncludeOtherTests)
            {
                failureOutcomes.AddRange(EnumHelper.GetEnumsExcept(TestOutcome.Failed, TestOutcome.NotExecuted, TestOutcome.Inconclusive));
            }

            return failureOutcomes;
        }
    }
}
