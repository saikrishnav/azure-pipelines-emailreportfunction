using System;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using System.Runtime.Serialization;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.ViewModel.Helpers;
using EmailReportFunction.Config;
using EmailReportFunction.Utils;

namespace EmailReportFunction.ViewModel
{
    [DataContract]
    public class TestResultSummaryViewModel
    {
        [DataMember]
        public string Duration { get; set; }

        [DataMember]
        public int FailedTests { get; set; }

        [DataMember]
        public int OtherTests { get; set; }

        [DataMember]
        public int PassedTests { get; set; }

        [DataMember]
        public string PassingRate { get; set; }

        [DataMember]
        public int TotalTests { get; set; }

        [DataMember]
        public string Url { get; set; }

        public TestResultSummaryViewModel(TestSummaryItem summaryItem, PipelineConfiguration pipelineConfiguration, bool includeOthersInTotal)
        {
            PassedTests = summaryItem.GetPassedTestsCount();
            FailedTests = summaryItem.GetFailedTestsCount();
            OtherTests = summaryItem.GetOtherTestsCount();

            TotalTests = TestResultsHelper.GetTotalTestCountBasedOnUserConfiguration(summaryItem.TestCountByOutCome,
                includeOthersInTotal);

            PassingRate = TestResultsHelper.GetTestOutcomePercentageString(PassedTests, TotalTests);

            Duration = TimeSpanFormatter.FormatDurationWithUnit(summaryItem.Duration);

            Url = pipelineConfiguration.TestTabLink;       
        }

        public TestResultSummaryViewModel(TestResultSummary summary, PipelineConfiguration pipelineConfiguration, bool includeOthersInTotal)
        {
            PassedTests = 0;
            FailedTests = 0;

            if (summary.AggregatedResultsAnalysis.ResultsByOutcome.ContainsKey(TestOutcome.Passed))
            {
                PassedTests = summary.AggregatedResultsAnalysis.ResultsByOutcome[TestOutcome.Passed].Count;
            }
            if (summary.AggregatedResultsAnalysis.ResultsByOutcome.ContainsKey(TestOutcome.Failed))
            {
                FailedTests = summary.AggregatedResultsAnalysis.ResultsByOutcome[TestOutcome.Failed].Count;
            }

            TotalTests = summary.AggregatedResultsAnalysis.TotalTests;
            OtherTests = TotalTests - PassedTests - FailedTests;

            if(!includeOthersInTotal)
            {
                TotalTests -= OtherTests;
            }

            PassingRate = TestResultsHelper.GetTestOutcomePercentageString(PassedTests, TotalTests);
            Duration = TimeSpanFormatter.FormatDurationWithUnit(summary.AggregatedResultsAnalysis.Duration);
            Url = pipelineConfiguration.TestTabLink;
        }
    }
}
