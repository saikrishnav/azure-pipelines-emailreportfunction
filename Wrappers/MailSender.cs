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

        public MailSender(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendMailAsync(IPipelineData pipelineData)
        {
            var smtpConfiguration = await CreateSmtpConfigurationAsync();
            var msg = CreateMailMessage(smtpConfiguration, pipelineData.ToString());//emailReportDto, emailReportConfig, config);
            return await SendMail(msg, smtpConfiguration);//, emailReportConfig.SmtpConfiguration);            
        }

        private MailMessage CreateMailMessage(SmtpConfiguration smtpConfiguration, string body) //EmailReportDto emailReportDto, EmailReportConfiguration emailReportConfig,           BaseConfiguration config)
        {
            // Create a message and set up the recipients.
            var message = new MailMessage(
            smtpConfiguration.UserName,
            "svajjala@microsoft.com",
            "[EmailTaskPPE] Test Azure Function", 
            body);

            //MailAddressViewModel mailAddressViewModel = GetMailAddressViewModel(emailReportDto, emailReportConfig);
            //msg.From = mailAddressViewModel.From;
            //msg.To.AddRange("svajjala@microsoft.com");
            //msg.CC.AddRange(mailAddressViewModel.Cc);

            //var emailReportViewModel = new EmailReportViewModel(emailReportDto, emailReportConfig, config);
            //Log.LogInfo("Generated view model");

            // msg.Subject =  emailReportViewModel.EmailSubject;

            // msg.Body = GenerateBodyFromViewModel(emailReportViewModel);

            return message;
        }

        private async Task<bool> SendMail(MailMessage msg, SmtpConfiguration smtpConfiguration) //, SmtpConfiguration smtpConfig)
        {
            try
            {
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
                    await smtpClient.SendMailAsync(msg);

                    return true;
                }
            }
            catch (Exception)
            {
                // ignore for now
            }
            return false;
        }

        private async Task<SmtpConfiguration> CreateSmtpConfigurationAsync()
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