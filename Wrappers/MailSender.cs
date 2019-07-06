using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.DataProviders;
using EmailReportFunction.Report;
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

        public MailSender(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendMailAsync(ReportMessage message)
        {
            try
            {
                var smtpInfo = new UriBuilder(message.SmtpConfiguration.SmtpHost);               
                using (var smtpClient = new SmtpClient(smtpInfo.Host))
                {
                    // if Url is provided in https://host:port format
                    if (!smtpInfo.Uri.IsDefaultPort)
                    {
                        smtpClient.Port = smtpInfo.Port;
                    }
                    // Enable SSL required for mail addresses outside corp net
                    smtpClient.EnableSsl = message.SmtpConfiguration.EnableSSL;
                    smtpClient.Credentials = new NetworkCredential(message.SmtpConfiguration.UserName, message.SmtpConfiguration.Password);

                    _logger.LogInformation($"Sending Mail using SmtpHost as '{smtpInfo.Host}:{smtpInfo.Port}' and EnableSSL as '{smtpClient.EnableSsl}'");
                    await smtpClient.SendMailAsync(message.MailMessage);

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