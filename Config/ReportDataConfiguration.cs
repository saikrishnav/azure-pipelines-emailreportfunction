using EmailReportFunction.Config.TestResults;

namespace EmailReportFunction.Config
{
    internal class ReportDataConfiguration
    {
        public bool IncludeCommits { get; set; }
        public bool IncludeOthersInTotal { get; set; }
        public bool IncludeResults { get; set; }
        public bool UsePreviousEnvironment { get; set; }
        public TestResultsGroupingType GroupTestSummaryBy { get; set; }
        public int MaxFailuresToShow { get; internal set; }
        public TestResultsGroupingType GroupTestResultsBy { get; internal set; }
    }
}