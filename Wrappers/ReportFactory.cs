using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.DataProviders;
using EmailReportFunction.Wrappers.Microsoft.EmailTask.EmailReport.Wrappers;
using Microsoft.Build.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.Wrappers
{
    public class ReportFactory : IReportFactory
    {
        public IMailSender GetMailSender(ILogger logger)
        {
            return new MailSender(logger);
        }

        public IDataProvider<IPipelineData> GetPipelineDataProvider(string jsonRequest, ILogger logger)
        {
            var pipelineConfiguration = CreatePipelineConfiguration(jsonRequest);
            if (pipelineConfiguration is ReleaseConfiguration)
            {
                var releaseConfig = pipelineConfiguration as ReleaseConfiguration;
                return new ReleaseDataProvider(releaseConfig, 
                    this.GetReleaseHttpClient(releaseConfig, logger),
                    this.GetFailedTestOwnersDataProvider(releaseConfig, logger),
                    logger);
            }
            return null;
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

        private IDataProvider<List<IdentityRef>> GetFailedTestOwnersDataProvider(ReleaseConfiguration releaseConfig, ILogger log)
        {
            var tcmClient = TestManagementHttpClientWrapper.CreateInstance(releaseConfig.Credentials, releaseConfig.ServerUri);
            var tcmApiHelper = new TcmApiHelper(tcmClient, null, log);
            ReleaseHttpClient releaseClient = null;
            if (!string.IsNullOrWhiteSpace(releaseConfig.RMServerUri) && releaseConfig.Credentials != null)
            {
                releaseClient = new ReleaseHttpClient(new Uri(releaseConfig.RMServerUri), releaseConfig.Credentials);
            }

            if (releaseClient == null)
            {
                log.LogError("ReleaseDatProvider: Unable to get ReleaseHttpClient");
            }

            return new FailedTestOwnersDataProvider(tcmApiHelper, log);
        }

        private PipelineConfiguration CreatePipelineConfiguration(string jsonRequest)
        {
            var data = (JObject)JsonConvert.DeserializeObject(jsonRequest);
            var pipelineType = data.GetValue("PipelineType").ToObject<PipelineType>();
            var accessToken = data.GetValue("System.AccessToken").ToString();
            var credentials = new VssBasicCredential("", accessToken);

            if (pipelineType == PipelineType.Release)
            {
                return new ReleaseConfiguration()
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
            }
            else
            {
                // TODO
                return null;
            }
        }
    }
}
