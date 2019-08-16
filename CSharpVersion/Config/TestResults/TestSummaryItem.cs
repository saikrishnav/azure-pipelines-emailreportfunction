using Microsoft.TeamFoundation.TestManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.TestResults
{
    public class TestSummaryItem
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public int TotalTests { get; set; }

        public Dictionary<TestOutcome, int> TestCountByOutCome { get; set; }

        public Dictionary<int, Dictionary<TestOutcomeForPriority, int>> TestCountForOutcomeByPriority { get; set; } = new Dictionary<int, Dictionary<TestOutcomeForPriority, int>>();

        public TimeSpan Duration { get; set; }
    }

    public enum TestOutcomeForPriority
    {
        Failed,
        Inconclusive,
        NotExecuted,
        Passed,
        Other
    }
}
