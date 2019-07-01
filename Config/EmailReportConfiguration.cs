using EmailReportFunction.Config.TestResults;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.Config
{
    public class EmailReportConfiguration
    {
        public PipelineConfiguration PipelineConfiguration { get; set; }

        public TestResultsGroupingType[] GroupTestSummaryBy { get; set; }

        public bool IncludeOthersInTotalCount { get; set; }

        public bool IncludeAssociatedChanges { get; set; }

        public bool IncludeEnvironmentInfo { get; set; }

        public TestResultsConfiguration TestResultsConfiguration { get; set; }

        public MailRecipientsConfiguration To { get; set; }

        public MailRecipientsConfiguration Cc { get; set; }

        public SendMailCondition SendMailCondition { get; set; }

        public string EmailSubject { get; set; }

        private ILogger _logger;

        public EmailReportConfiguration(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<SmtpConfiguration> GetSmtpConfigurationAsync()
        {
            var reportInputs = this.GetReportInputs();
            var secret = await KeyVaultReader.FetchSecret(reportInputs.KeyVaultName,
                reportInputs.SecretName, reportInputs.RetryCount, _logger);

            return new SmtpConfiguration()
            {
                EnableSSL = true,
                SmtpHost = reportInputs.SmtpHost,
                UserName = reportInputs.MailSenderAddress,
                Password = secret.Value
            };
        }

        private ReportInputs GetReportInputs()
        {
            return new ReportInputs()
            {
                KeyVaultName = "vstsbuild",
                SecretName = "azpipes",
                MailSenderAddress = $"azpipes@microsoft.com",
                SmtpHost = "https://smtp.office365.com",
                RetryCount = 3
            };
        }
    }
}
