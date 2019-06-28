using EmailReportFunction.Config.TestResults;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config
{
    public class EmailReportConfiguration
    {
        public TestResultsGroupingType[] GroupTestSummaryBy { get; set; }

        public bool IncludeOthersInTotalCount { get; set; }

        public bool IncludeAssociatedChanges { get; set; }

        public bool IncludeEnvironmentInfo { get; set; }

        public TestResultsConfiguration TestResultsConfiguration { get; set; }

        public MailRecipientsConfiguration To { get; set; }

        public MailRecipientsConfiguration Cc { get; set; }

        public SendMailCondition SendMailCondition { get; set; }

        public string EmailSubject { get; set; }

        public SmtpConfiguration SmtpConfiguration { get; set; }

        public string ProjectId { get; set; }
    }
}
