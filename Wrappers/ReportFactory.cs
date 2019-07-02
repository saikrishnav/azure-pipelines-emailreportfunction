using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.DataProviders;
using EmailReportFunction.Wrappers.Microsoft.EmailTask.EmailReport.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmailReportFunction.Wrappers
{
    public class ReportFactory : IReportFactory
    {
        public IMailSender GetMailSender(string jsonRequest, ILogger logger)
        {
            return new MailSender(CreateEmailReportConfiguration(jsonRequest, logger), logger);
        }

        private EmailReportConfiguration _reportConfiguration;

        public IDataProvider<IPipelineData> GetPipelineDataProvider(string jsonRequest, ILogger logger)
        {
            var emailReportConfiguration = CreateEmailReportConfiguration(jsonRequest, logger);
            var pipelineConfiguration = emailReportConfiguration.PipelineConfiguration;
            if (pipelineConfiguration is ReleaseConfiguration)
            {
                var releaseConfig = pipelineConfiguration as ReleaseConfiguration;
                return new ReleaseDataProvider(releaseConfig, 
                    this.GetReleaseHttpClient(releaseConfig, logger),
                    this.GetFailedTestOwnersDataProvider(emailReportConfiguration, logger),
                    this.GetTestResultsDataProvider(emailReportConfiguration, logger),
                    this.GetTestSummaryDataProvider(emailReportConfiguration, logger),
                    logger);
            }
            return null;
        }

        private TestSummaryDataProvider GetTestSummaryDataProvider(EmailReportConfiguration emailReportConfiguration, ILogger logger)
        {
            var tcmApiHelper = GetTcmApiHelper(emailReportConfiguration, logger);
            return new TestSummaryDataProvider(tcmApiHelper, emailReportConfiguration, logger);
        }

        private IDataProvider<List<IdentityRef>> GetFailedTestOwnersDataProvider(EmailReportConfiguration emailReportConfiguration, ILogger logger)
        {
            var tcmApiHelper = GetTcmApiHelper(emailReportConfiguration, logger);
            return new FailedTestOwnersDataProvider(tcmApiHelper, logger);
        }

        private IDataProvider<IEnumerable<TestResultsGroupData>> GetTestResultsDataProvider(EmailReportConfiguration emailReportConfiguration, ILogger log)
        {
            var witHelper = GetWorkItemTrackingApiHelper(emailReportConfiguration.PipelineConfiguration, log);
            var tcmApiHelper = GetTcmApiHelper(emailReportConfiguration, log);
            return new TestResultsDataProvider(tcmApiHelper, witHelper, emailReportConfiguration.TestResultsConfiguration, log);
        }

        private ITcmApiHelper GetTcmApiHelper(EmailReportConfiguration emailReportConfiguration, ILogger logger)
        {
            var pipelineConfiguration = emailReportConfiguration.PipelineConfiguration;
            var tcmClient = TestManagementHttpClientWrapper.CreateInstance(pipelineConfiguration.Credentials, pipelineConfiguration.ServerUri);
            if (pipelineConfiguration is ReleaseConfiguration)
            {
                return new ReleaseTcmApiHelper(tcmClient, emailReportConfiguration, logger);
            }
            else
            {
                // TODO - Build
                return null;
            }
        }

        private IReleaseHttpClientWrapper GetReleaseHttpClient(ReleaseConfiguration releaseConfig, ILogger log)
        {
            ReleaseHttpClient releaseClient = null;
            if (!string.IsNullOrWhiteSpace(releaseConfig.RMServerUri) && releaseConfig.Credentials != null)
            {
                releaseClient = new ReleaseHttpClient(new Uri(releaseConfig.RMServerUri), releaseConfig.Credentials);
            }

            if (releaseClient == null)
            {
                log.LogError("ReleaseDatProvider: Unable to get ReleaseHttpClient");
            }

            return new ReleaseHttpClientWrapper(releaseClient);
        }

        private IWorkItemTrackingApiHelper GetWorkItemTrackingApiHelper(PipelineConfiguration pipelineConfiguration, ILogger log)
        {
            IWorkItemTrackingApiHelper workItemTrackingApiHelper = null;
            if (!string.IsNullOrWhiteSpace(pipelineConfiguration.ServerUri) && pipelineConfiguration.Credentials != null)
            {
                var client = new WorkItemTrackingHttpClient(new Uri(pipelineConfiguration.ServerUri), pipelineConfiguration.Credentials);
                if (client == null)
                {
                    log.LogError("ReleaseDatProvider: Unable to get WorkItemTrackingHttpClient");
                }
                workItemTrackingApiHelper = new WorkItemTrackingApiHelper(client);
            }

            return workItemTrackingApiHelper;
        }

        private EmailReportConfiguration CreateEmailReportConfiguration(string jsonRequest, ILogger logger)
        {
            if (_reportConfiguration == null)
            {
                var data = (JObject)JsonConvert.DeserializeObject(jsonRequest);
                var pipelineType = data.GetValue("PipelineType").ToObject<PipelineType>();
                var accessToken = data.GetValue("System.AccessToken").ToString();
                var credentials = new VssBasicCredential("", accessToken);

                //TODO  -default
                var testResultsConfig = new TestResultsConfiguration()
                {
                    GroupingType = TestResultsGroupingType.Run,
                    IncludeFailedTests = true,
                    MaxItemsToShow = 10
                };

                if (pipelineType == PipelineType.Release)
                {
                    var releaseConfig = new ReleaseConfiguration()
                    {
                        ProjectId = data.GetValue("ProjectId").ToString(),
                        ProjectName = data.GetValue("ProjectName").ToString(),
                        ReleaseId = data.GetValue("ReleaseId").ToObject<int>(),
                        ServerUri = data.GetValue("ServerUri").ToString(),
                        RMServerUri = data.GetValue("RMServerUri").ToString(),
                        DefinitionEnvironmentId = data.GetValue("DefinitionEnvironmentId").ToObject<int>(),
                        EnvironmentId = data.GetValue("EnvironmentId").ToObject<int>(),
                        Credentials = credentials
                    };

                    _reportConfiguration = new EmailReportConfiguration(logger)
                    {
                        To = new MailRecipientsConfiguration() { DefaultRecipients = "svajjala@microsoft.com" },
                        Cc = new MailRecipientsConfiguration(),
                        GroupTestSummaryBy = new TestResultsGroupingType[] { TestResultsGroupingType.Run },
                        PipelineConfiguration = releaseConfig,
                        EmailSubject = "Azure Function Email Task",
                        SendMailCondition = SendMailCondition.Always,
                        TestResultsConfiguration = testResultsConfig
                    };
                }
                else
                {
                    // TODO
                    return null;
                }
            }
            return _reportConfiguration;
        }

        public IReportMessageGenerator GetReportMessageGenerator(string jsonRequest, ILogger logger)
        {
            return new ReportMessageGenerator(CreateEmailReportConfiguration(jsonRequest, logger), logger);
        }
    }
}
