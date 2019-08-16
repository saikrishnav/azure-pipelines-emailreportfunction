using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.PostProcessor;
using EmailReportFunction.Wrappers;
using EmailReportFunction.Wrappers.Microsoft.EmailTask.EmailReport.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.DataProviders
{
    public class DataProviderFactory : IDataProviderFactory
    {
        private EmailReportConfiguration _emailReportConfiguration;
        private ILogger _logger;

        public DataProviderFactory(EmailReportConfiguration emailReportConfig, ILogger logger)
        {
            _emailReportConfiguration = emailReportConfig;
            _logger = logger;
        }

        public IEnumerable<IDataProvider> GetPipelineDataProviders()
        {
            return new List<IDataProvider>()
            {
                GetPipelineSpecificDataProvider(),
                GetFailedTestOwnersDataProvider(),
                GetTestSummaryDataProvider(),
                GetTestResultsDataProvider(),
                this.SmtpDataProvider
            };
        }

        public IEnumerable<IDataProvider> GetPostProcessors()
        {
            return new List<IDataProvider>() { this.SendMailConditionPostProcessor };
        }


        private IDataProvider _sendMailConditionPostProcessor;
        public IDataProvider SendMailConditionPostProcessor
        {
            get
            {
                if (_sendMailConditionPostProcessor == null)
                {
                    _sendMailConditionPostProcessor = new SendMailConditionPostProcessor(this._emailReportConfiguration, this.GetTcmApiHelper(), _logger);
                }
                return _sendMailConditionPostProcessor;
            }
        }

        private IDataProvider _smtpDataProvider;
        private IDataProvider SmtpDataProvider
        {
            get
            {
                if (_smtpDataProvider == null)
                {
                    _smtpDataProvider = new SmtpConfigurationProvider(_logger);
                }
                return _smtpDataProvider;
            }
        }

        private ITcmApiHelper _tcmApiHelper;
        private ITcmApiHelper TcmApiHelper
        {
            get
            {
                if (_tcmApiHelper == null)
                {
                    var pipelineConfiguration = _emailReportConfiguration.PipelineConfiguration;
                    var tcmClient = TestManagementHttpClientWrapper.CreateInstance(pipelineConfiguration.Credentials, pipelineConfiguration.ServerUri);
                    if (pipelineConfiguration is ReleaseConfiguration)
                    {
                        _tcmApiHelper = new ReleaseTcmApiHelper(tcmClient, _emailReportConfiguration, _logger);
                    }
                    else
                    {
                        // TODO - Build
                        return null;
                    }
                }
                return _tcmApiHelper;
            }
        }

        public IDataProvider GetPipelineSpecificDataProvider()
        {
            var pipelineConfiguration = _emailReportConfiguration.PipelineConfiguration;
            if (pipelineConfiguration is ReleaseConfiguration)
            {
                var releaseConfig = pipelineConfiguration as ReleaseConfiguration;
                return new ReleaseDataProvider(
                    releaseConfig, 
                    this.GetReleaseHttpClient(),
                    _logger);
            }
            return null;
        }

        private IDataProvider GetTestSummaryDataProvider()
        {
            return new TestSummaryDataProvider(this.TcmApiHelper, _emailReportConfiguration.ReportDataConfiguration, _logger);
        }

        private IDataProvider GetFailedTestOwnersDataProvider()
        {
            return new FailedTestOwnersDataProvider(this.TcmApiHelper, _logger);
        }

        private IDataProvider GetTestResultsDataProvider()
        {
            var witHelper = GetWorkItemTrackingApiHelper(_emailReportConfiguration.PipelineConfiguration, _logger);
            var tcmApiHelper = GetTcmApiHelper();
            return new TestResultsDataProvider(tcmApiHelper, witHelper, _emailReportConfiguration.ReportDataConfiguration, _logger);
        }

        private ITcmApiHelper GetTcmApiHelper()
        {
            var pipelineConfiguration = _emailReportConfiguration.PipelineConfiguration;
            var tcmClient = TestManagementHttpClientWrapper.CreateInstance(pipelineConfiguration.Credentials, pipelineConfiguration.ServerUri);
            if (pipelineConfiguration is ReleaseConfiguration)
            {
                return new ReleaseTcmApiHelper(tcmClient, _emailReportConfiguration, _logger);
            }
            else
            {
                // TODO - Build
                return null;
            }
        }

        private IReleaseHttpClientWrapper GetReleaseHttpClient()
        {
            if (_emailReportConfiguration.PipelineConfiguration is ReleaseConfiguration)
            {
                var releaseConfig = _emailReportConfiguration.PipelineConfiguration as ReleaseConfiguration;
                ReleaseHttpClient releaseClient = null;
                if (!string.IsNullOrWhiteSpace(releaseConfig.RMServerUri) && releaseConfig.Credentials != null)
                {
                    releaseClient = new ReleaseHttpClient(new Uri(releaseConfig.RMServerUri), releaseConfig.Credentials);
                }

                if (releaseClient == null)
                {
                    _logger.LogError("ReleaseDatProvider: Unable to get ReleaseHttpClient");
                }

                return new ReleaseHttpClientWrapper(releaseClient);
            }
            return null;
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
    }
}
