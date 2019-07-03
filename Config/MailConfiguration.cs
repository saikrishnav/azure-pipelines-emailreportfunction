using EmailReportFunction.Config;

namespace EmailReportFunction.Config
{
    public class MailConfiguration
    {
        public const string SecretName = "azpipes";

        public static readonly string MailSenderAddress = $"{SecretName}@microsoft.com";

        public const string KeyVaultName = "vstsbuild";

        public const string SmtpHost = "https://smtp.office365.com";

        public const int RetryCount = 3;

        public SendMailCondition SendMailCondition { get; set; }

        public string ToAddrresses { get; set; }

        public string IncludeInTo { get; set; }

        public string CcAddrresses { get; set; }

        public string IncludeInCc { get; set; }

        public string EmailSubject { get; set; }

        private MailRecipientsConfiguration _to;
        public MailRecipientsConfiguration To
        {
            get
            {
                if (_to == null)
                {
                    _to = GetRecipientsConfiguration(ToAddrresses, IncludeInTo ?? string.Empty);
                }
                return _to;
           }
        }

        private MailRecipientsConfiguration _cc;
        public MailRecipientsConfiguration Cc
        {
            get
            {
                if (_cc == null)
                {
                    _cc = GetRecipientsConfiguration(CcAddrresses, IncludeInCc ?? string.Empty);
                }
                return _cc;
            }
        }

        private static MailRecipientsConfiguration GetRecipientsConfiguration(string defaultAddresses, string includeConfig)
        {
            if (string.IsNullOrWhiteSpace(defaultAddresses))
            {
                return MailRecipientsConfiguration.Empty;
            }

            return new MailRecipientsConfiguration()
            {
                DefaultRecipients = defaultAddresses,
                IncludeChangesetOwners = includeConfig.Contains("1"),
                IncludeTestOwners = includeConfig.Contains("2"),
                IncludeActiveBugOwners = includeConfig.Contains("3"),
                IncludeCreatedBy = includeConfig.Contains("4")
            };
        }
    }
}