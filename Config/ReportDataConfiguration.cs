using EmailReportFunction.Config.TestResults;

namespace EmailReportFunction.Config
{
    public class ReportDataConfiguration
    {
        public bool IncludeCommits { get; set; }
        public bool IncludeOthersInTotal { get; set; }
        public string IncludeResults { get; set; }
        public bool UsePreviousEnvironment { get; set; }
        public TestResultsGroupingType[] GroupTestSummaryBy { get; set; }
        public int MaxFailuresToShow { get; internal set; }
        public TestResultsGroupingType GroupTestResultsBy { get; internal set; }

        public bool IncludeFailedTests => IncludeResults.Contains("1");
        public bool IncludeOtherTests => IncludeResults.Contains("2");

        // TODO - where is this used?
        public bool IncludePassedTests { get; internal set; }
    }
}