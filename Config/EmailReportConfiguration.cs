using EmailReportFunction.Config.Pipeline;

namespace EmailReportFunction.Config
{
    public class EmailReportConfiguration
    {
        public PipelineType PipelineType { get; set; }
         
        public PipelineConfiguration PipelineConfiguration { get; set; }

        public MailConfiguration MailConfiguration { get; set; }

        public ReportDataConfiguration ReportDataConfiguration { get; set; }

        //public TestResultsGroupingType[] GroupTestSummaryBy { get; set; }

        //public bool IncludeOthersInTotalCount { get; set; }

        //public bool IncludeAssociatedChanges { get; set; }

        //public bool IncludeEnvironmentInfo { get; set; }

        //public TestResultsConfiguration TestResultsConfiguration { get; set; }

        //public MailRecipientsConfiguration To { get; set; }

        //public MailRecipientsConfiguration Cc { get; set; }

        //public SendMailCondition SendMailCondition { get; set; }

        //public string EmailSubject { get; set; }
    }
}
