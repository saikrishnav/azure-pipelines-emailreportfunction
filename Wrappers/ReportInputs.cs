using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction
{
    public class ReportInputs
    {
        public string KeyVaultName { get; set; }

        public string SecretName { get; set; }

        public string MailSenderAddress { get; set; }

        public int RetryCount { get; set; }

        public string SmtpHost { get; set; }
    }
}
