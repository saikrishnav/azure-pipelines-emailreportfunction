using EmailReportFunction.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.DataProviders
{
    public class SmtpConfigurationProvider : IDataProvider<SmtpConfiguration>
    {
        private ILogger _logger;

        public SmtpConfigurationProvider(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<SmtpConfiguration> GetDataAsync()
        {
            var secret = await KeyVaultReader.FetchSecret(
                MailConfiguration.KeyVaultName,
                MailConfiguration.SecretName,
                MailConfiguration.RetryCount, 
                _logger);

            return new SmtpConfiguration()
            {
                EnableSSL = true,
                SmtpHost = MailConfiguration.SmtpHost,
                UserName = MailConfiguration.MailSenderAddress,
                Password = secret.Value
            };
        }
    }
}
