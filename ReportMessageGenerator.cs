using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.Exceptions;
using EmailReportFunction.Utils;
using Microsoft.Extensions.Logging;
using EmailReportFunction.ViewModel;
using Microsoft.VisualStudio.Services.Common;
using Artifact = Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts.Artifact;

namespace EmailReportFunction
{
    public class ReportMessageGenerator : IReportMessageGenerator
    {
        private ILogger _logger;
        private EmailReportConfiguration _emailReportConfiguration;

        public ReportMessageGenerator(EmailReportConfiguration emailReportConfiguration, ILogger logger)
        {
            _logger = logger;
            _emailReportConfiguration = emailReportConfiguration;
        }

        public async Task<MailMessage> GenerateReportAsync(IPipelineData pipelineData)
        {
            var emailReportDto = new ReleaseEmailReportDto();
            var releaseData = pipelineData as ReleaseData;
            emailReportDto.FailedTestOwners = await pipelineData.GetFailedTestOwnersAsync();
            var filteredResults = await pipelineData.GetFilteredTestsAsync();
            emailReportDto.FilteredResults = filteredResults == null ? new List<Config.TestResults.TestResultsGroupData>() : filteredResults.ToList();
            emailReportDto.Artifacts = new List<Artifact>(releaseData.Release.Artifacts);
            emailReportDto.Release = releaseData.Release;
            emailReportDto.Environment = releaseData.Environment;
            emailReportDto.Phases = await releaseData.GetPhasesAsync();
            emailReportDto.AssociatedChanges = await releaseData.GetAssociatedChangesAsync();
            emailReportDto.LastCompletedRelease = await releaseData.GetLastCompletedReleaseAsync();
            emailReportDto.LastCompletedEnvironment = await releaseData.GetLastCompletedEnvironmentAsync();
            emailReportDto.CreatedBy = releaseData.Release.CreatedBy;

            var summaryData = await pipelineData.GetTestSummaryDataAsync();
            emailReportDto.Summary = summaryData.ResultSummary;
            emailReportDto.TestSummaryGroups = summaryData.TestSummaryGroups;
            // TODO - Has FilteredResults

            var msg = new System.Net.Mail.MailMessage { IsBodyHtml = true };

            var mailAddressViewModel = new MailAddressViewModel(_emailReportConfiguration.MailConfiguration, pipelineData, _logger);
            var recipients = await mailAddressViewModel.GetRecipientAdrressesAsync();
            msg.From = mailAddressViewModel.From;
            msg.To.AddRange(recipients[RecipientType.TO]);
            msg.CC.AddRange(recipients[RecipientType.CC]);

            _logger.LogInformation("Sending mail for to address - " +
                           string.Join(";", msg.To.Select(mailAddress => mailAddress.Address)));

            _logger.LogInformation("Sending mail for cc address - " +
                          string.Join(";", msg.CC.Select(mailAddress => mailAddress.Address)));

            _logger.LogInformation("Creating view model for generating xml");
            var emailReportViewModel = new EmailReportViewModel(emailReportDto, _emailReportConfiguration);
            _logger.LogInformation("Generated view model");

            msg.Subject = emailReportViewModel.EmailSubject;

            msg.Body = GenerateBodyFromViewModel(emailReportViewModel);

            return msg;
        }

        protected virtual string GenerateBodyFromViewModel(EmailReportViewModel viewModel)
        {
            using (new PerformanceMeasurementBlock("Generating Html content for message body", _logger))
            {
                var xml = GetXmlContent(viewModel);

                _logger.LogInformation("Generating html content for email body");

                return GetHtml(xml);
            }
        }

        private string GetHtml(string xml)
        {
            var templateName = "EmailReportFunction.EmailTemplate.xslt";
            var executingAssembly = Assembly.GetExecutingAssembly();

            _logger.LogInformation("Loading Email Template");
            var xslStream = executingAssembly.GetManifestResourceStream(templateName);

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

        private string GetXmlContent(EmailReportViewModel viewModel)
        {
            _logger.LogInformation("Generating xml from view model");

            var xsSubmit = new DataContractSerializer(typeof(EmailReportViewModel));
            using (var stringWriter = new StringWriter())
            using (var writer = new XmlTextWriterWithoutNamespace(stringWriter))
            {
                try
                {
                    xsSubmit.WriteObject(writer, viewModel);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                _logger.LogInformation($"Generated xml content from view model - {stringWriter}");
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
