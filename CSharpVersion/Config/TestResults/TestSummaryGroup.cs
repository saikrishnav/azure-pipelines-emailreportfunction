using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.TestResults
{
    public class TestSummaryGroup
    {
        public TestResultsGroupingType GroupingType { get; set; }

        public List<TestSummaryItem> Runs { get; set; } = new List<TestSummaryItem>();
    }
}
