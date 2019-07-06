using EmailReportFunction.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.DataProviders
{
    public class SmtpConfigurationProvider : IDataProvider
    {
        private ILogger _logger;

        public SmtpConfigurationProvider(ILogger logger)
        {
            _logger = logger;
        }

        public async Task AddReportDataAsync(AbstractReport reportData)
        {
            var secret = await KeyVaultReader.FetchSecret(
                MailConfiguration.KeyVaultName,
                MailConfiguration.SecretName,
                MailConfiguration.RetryCount, 
                _logger);

            reportData.SmtpConfiguration = new SmtpConfiguration()
            {
                EnableSSL = true,
                SmtpHost = MailConfiguration.SmtpHost,
                UserName = MailConfiguration.MailSenderAddress,
                Password = secret.Value
            };
        }
    }
}
