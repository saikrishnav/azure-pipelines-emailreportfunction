using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using EmailReportFunction.Config.Pipeline;
using Microsoft.Extensions.Logging;

namespace EmailReportFunction
{
    public class ReportMessageGenerator : IReportMessageGenerator
    {
        private ILogger _logger;
        public ReportMessageGenerator(ILogger logger)
        {
            _logger = logger;
        }

        public MailMessage GenerateReport(IPipelineData pipelineData)
        {
            var msg = new MailMessage { IsBodyHtml = true };

            MailAddressViewModel mailAddressViewModel = GetMailAddressViewModel(emailReportDto, emailReportConfig);
            msg.From = mailAddressViewModel.From;
            msg.To.AddRange(mailAddressViewModel.To);
            msg.CC.AddRange(mailAddressViewModel.Cc);

            _logger.LogInformation("Sending mail for to address - " +
                           string.Join(";", msg.To.Select(mailAddress => mailAddress.Address)));

            _logger.LogInformation("Sending mail for cc address - " +
                          string.Join(";", msg.CC.Select(mailAddress => mailAddress.Address)));

            _logger.LogInformation("Creating view model for generating xml");
            var emailReportViewModel = new EmailReportViewModel(emailReportDto, emailReportConfig, config);
            _logger.LogInformation("Generated view model");

            msg.Subject = emailReportViewModel.EmailSubject;

            msg.Body = GenerateBodyFromViewModel(emailReportViewModel);

            return msg;
        }

        /// <summary>
        ///     For testing purpose
        /// </summary>
        protected virtual MailAddressViewModel GetMailAddressViewModel(EmailReportDto emailReportDto,
            EmailReportConfiguration emailReportConfig)
        {
            return new MailAddressViewModel(emailReportDto, emailReportConfig);
        }

        protected virtual string GenerateBodyFromViewModel(EmailReportViewModel viewModel)
        {
            using (new PerformanceMeasurementBlock("Generating Html content for message body"))
            {
                var xml = GetXmlContent(viewModel);

                Log.LogInfo("Generating html content for email body");

                return GetHtml(xml);
            }
        }

        private static string GetHtml(string xml)
        {
            var templateName = "Microsoft.EmailTask.EmailReport.EmailTemplate.xslt";
            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            Log.LogVerbose("Loading Email Template");
            Stream xslStream = executingAssembly.GetManifestResourceStream(templateName);

            if (xslStream == null)
            {
                throw new EmailReportException("Unable to get xsl template for email");
            }

            using (var sr = new StreamReader(xslStream))
            {
                XDocument doc = XDocument.Parse(xml);

                return Transform(doc, sr);
            }
        }

        private static string GetXmlContent(EmailReportViewModel viewModel)
        {
            Log.LogInfo("Generating xml from view model");

            var xsSubmit = new DataContractSerializer(typeof(EmailReportViewModel));
            using (var stringWriter = new StringWriter())
            using (var writer = new XmlTextWriterWithoutNamespace(stringWriter))
            {
                xsSubmit.WriteObject(writer, viewModel);

                Log.LogVerbose($"Generated xml content from view model - {stringWriter}");
                return stringWriter.ToString();
            }
        }

        private string Transform(XDocument xDocument, StreamReader xsltStream)
        {
            using (TextWriter textWriter = new StringWriter())
            {
                var transform = new XslCompiledTransform();
                using (XmlReader xsltReader = new XmlTextReader(xsltStream))
                {
                    transform.Load(xsltReader);
                }
                transform.Transform(xDocument.CreateReader(), null, textWriter);
                textWriter.Flush();

                _logger.LogInformation("successfully generated html content for email body");
                _logger.LogInformation($"Generated html content for email body - {textWriter}");

                return textWriter.ToString();
            }
        }
    }
}
