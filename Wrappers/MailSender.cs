using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace EmailReportFunction.Wrappers
{
    public class MailSender : IMailSender
    {
        private ILogger _logger;
        private EmailReportConfiguration _emailReportConfiguration;

        public MailSender(EmailReportConfiguration reportConfig, ILogger logger)
        {
            _logger = logger;
            _emailReportConfiguration = reportConfig;
        }

        public async Task<bool> SendMailAsync(MailMessage message)
        {
            try
            {
                var smtpConfiguration = await _emailReportConfiguration.GetSmtpConfigurationAsync();
                var smtpInfo = new UriBuilder(smtpConfiguration.SmtpHost);               
                using (var smtpClient = new SmtpClient(smtpInfo.Host))
                {
                    // if Url is provided in https://host:port format
                    if (!smtpInfo.Uri.IsDefaultPort)
                    {
                        smtpClient.Port = smtpInfo.Port;
                    }
                    // Enable SSL required for mail addresses outside corp net
                    smtpClient.EnableSsl = smtpConfiguration.EnableSSL;
                    smtpClient.Credentials = new NetworkCredential(smtpConfiguration.UserName, smtpConfiguration.Password);

                    _logger.LogInformation($"Sending Mail using SmtpHost as '{smtpInfo.Host}:{smtpInfo.Port}' and EnableSSL as '{smtpClient.EnableSsl}'");
                    await smtpClient.SendMailAsync(message);

                    return true;
                }
            }
            catch (Exception)
            {
                // ignore for now
            }
            return false;
        }
    }
}