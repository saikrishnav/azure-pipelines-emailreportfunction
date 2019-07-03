using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config
{
    public class MailRecipientsConfiguration
    {
        public string DefaultRecipients { get; set; }
        public bool IncludeChangesetOwners { get; set; }
        public bool IncludeTestOwners { get; set; }
        public bool IncludeActiveBugOwners { get; set; }
        public bool IncludeCreatedBy { get; set; }

        public static readonly MailRecipientsConfiguration Empty = new MailRecipientsConfiguration();
    }
}
