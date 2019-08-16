using EmailReportFunction.Config.TestResults;
using System.Runtime.Serialization;

namespace EmailReportFunction.Config
{
    [DataContract]
    public class ReportDataConfiguration
    {
        [DataMember]
        public bool IncludeCommits { get; set; }

        [DataMember]
        public bool IncludeOthersInTotal { get; set; }

        [DataMember]
        public string IncludeResults { get; set; }

        [DataMember]
        public bool UsePreviousEnvironment { get; set; }

        [DataMember]
        public TestResultsGroupingType GroupTestSummaryBy { get; set; }

        [DataMember]
        public int MaxFailuresToShow { get; set; }

        [DataMember]
        public TestResultsGroupingType GroupTestResultsBy { get; set; }

        public bool IncludeFailedTests => IncludeResults.Contains("1");
        public bool IncludeOtherTests => IncludeResults.Contains("2");

        // TODO - where is this used?
        public bool IncludePassedTests { get; internal set; }
    }
}