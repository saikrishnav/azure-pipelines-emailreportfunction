using System;
using Microsoft.EmailTask.EmailReport.Config;
using Microsoft.EmailTask.EmailReport.Dto;
using Microsoft.EmailTask.EmailReport.Utils;
using Microsoft.EmailTask.EmailReport.ViewModel.Helpers;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using System.Runtime.Serialization;

namespace Microsoft.EmailTask.EmailReport.ViewModel
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

        public TestResultSummaryViewModel(TestSummaryItemDto summaryItemDto, BaseConfiguration config, bool includeOthersInTotal)
        {
            PassedTests = summaryItemDto.GetPassedTestsCount();
            FailedTests = summaryItemDto.GetFailedTestsCount();
            OtherTests = summaryItemDto.GetOtherTestsCount();

            TotalTests = TestResultsHelper.GetTotalTestCountBasedOnUserConfiguration(summaryItemDto.TestCountByOutCome,
                includeOthersInTotal);

            PassingRate = TestResultsHelper.GetTestOutcomePercentageString(PassedTests, TotalTests);

            Duration = TimeSpanFormatter.FormatDurationWithUnit(summaryItemDto.Duration);

            Url = config.GetTestTabLink();       
        }

        public TestResultSummaryViewModel(TestResultSummary summary, BaseConfiguration config, bool includeOthersInTotal)
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
            Url = config.GetTestTabLink();
        }
    }
}
