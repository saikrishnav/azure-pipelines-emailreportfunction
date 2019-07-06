using EmailReportFunction.Config;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace EmailReportFunction.Report
{
    public class ReportMessage
    {
        public SmtpConfiguration SmtpConfiguration { get; set; }

        public MailMessage MailMessage { get; set; }
    }
}
