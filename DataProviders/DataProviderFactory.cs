using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
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

        public IDataProvider<T> GetDataProvider<T>()
        {
            IDataProvider<T> dataProvider = null;
            if (typeof(T) == typeof(SmtpConfiguration))
            {
                dataProvider = (IDataProvider<T>) SmtpDataProvider;
            }
            else if (typeof(T) == typeof(IPipelineData))
            {
                dataProvider = (IDataProvider<T>) this.GetPipelineDataProvider();
            }
            else if (typeof(T) == typeof(List<IdentityRef>))
            {
                dataProvider = (IDataProvider<T>)this.GetFailedTestOwnersDataProvider();
            }
            else if (typeof(T) == typeof(IDataProvider<TestSummaryData>))
            {
                dataProvider = (IDataProvider<T>) this.GetTestSummaryDataProvider();
            }
            else if (typeof(T) == typeof(IDataProvider<FilteredTestResultData>))
            {
                dataProvider = (IDataProvider<T>)this.GetTestResultsDataProvider();
            }

            return dataProvider;
        }

        private IDataProvider<SmtpConfiguration> _smtpDataProvider;
        private IDataProvider<SmtpConfiguration> SmtpDataProvider
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

        public IDataProvider<IPipelineData> GetPipelineDataProvider()
        {
            var pipelineConfiguration = _emailReportConfiguration.PipelineConfiguration;
            if (pipelineConfiguration is ReleaseConfiguration)
            {
                var releaseConfig = pipelineConfiguration as ReleaseConfiguration;
                return new ReleaseDataProvider(
                    releaseConfig, 
                    this.GetReleaseHttpClient(),
                    this.GetFailedTestOwnersDataProvider(),
                    this.GetTestResultsDataProvider(),
                    this.GetTestSummaryDataProvider(),
                    _logger);
            }
            return null;
        }

        private IDataProvider<TestSummaryData> GetTestSummaryDataProvider()
        {
            return new TestSummaryDataProvider(this.TcmApiHelper, _emailReportConfiguration.ReportDataConfiguration, _logger);
        }

        private IDataProvider<List<IdentityRef>> GetFailedTestOwnersDataProvider()
        {
            return new FailedTestOwnersDataProvider(this.TcmApiHelper, _logger);
        }

        private IDataProvider<FilteredTestResultData> GetTestResultsDataProvider()
        {
            var witHelper = GetWorkItemTrackingApiHelper(_emailReportConfiguration.PipelineConfiguration, _logger);
            var tcmApiHelper = GetTcmApiHelper(_emailReportConfiguration, _logger);
            return new TestResultsDataProvider(tcmApiHelper, witHelper, _emailReportConfiguration.ReportDataConfiguration, _logger);
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
