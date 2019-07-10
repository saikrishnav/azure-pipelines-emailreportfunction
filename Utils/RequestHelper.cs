using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.Config.TestResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.Services.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailReportFunction.Utils
{
    public static class RequestHelper
    {
        public static EmailReportConfiguration CreateConfiguration(IHeaderDictionary headers, string jsonRequest, ILogger logger)
        {
            var data = (JObject)JsonConvert.DeserializeObject(jsonRequest);
            StringValues authTokenValues;
            headers.TryGetValue("AuthToken", out authTokenValues);
            var accessToken = authTokenValues.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var credentials = new VssBasicCredential("", accessToken);

                var reportDataConfigurationObject = (JObject)JsonConvert.DeserializeObject(data.GetValue("ReportDataConfiguration").ToString());
                var reportDataConfiguration = new ReportDataConfiguration()
                {
                    IncludeCommits = reportDataConfigurationObject.GetJsonValue<bool>("IncludeCommits"),
                    IncludeOthersInTotal = reportDataConfigurationObject.GetJsonValue<bool>("IncludeOthersInTotal"),
                    IncludeResults = reportDataConfigurationObject.GetJsonValue<string>("IncludeResults"),
                    UsePreviousEnvironment = reportDataConfigurationObject.GetJsonValue<bool>("UsePreviousEnvironment"),
                    GroupTestSummaryBy = new TestResultsGroupingType[] { reportDataConfigurationObject.GetJsonValue<TestResultsGroupingType>("GroupTestSummaryBy") },
                    MaxFailuresToShow = reportDataConfigurationObject.GetJsonValue<int>("MaxFailuresToShow"),
                    GroupTestResultsBy = reportDataConfigurationObject.GetJsonValue<TestResultsGroupingType>("GroupTestResultsBy")
                };

                var mailConfigurationObject = (JObject)JsonConvert.DeserializeObject(data.GetValue("EmailConfiguration").ToString());
                var mailConfiguration = new MailConfiguration()
                {
                    EmailSubject = mailConfigurationObject.GetJsonValue<string>("MailSubject"),
                    SendMailCondition = mailConfigurationObject.GetJsonValue<SendMailCondition>("SendMailCondition"),
                    ToAddrresses = mailConfigurationObject.GetJsonValue<string>("ToAddresses"),
                    IncludeInTo = mailConfigurationObject.GetJsonValue<string>("IncludeInTo"),
                    CcAddrresses = mailConfigurationObject.GetJsonValue<string>("CcAddrresses"),
                    IncludeInCc = mailConfigurationObject.GetJsonValue<string>("IncludeInCc"),
                };

                var pipelineInfoObject = (JObject)JsonConvert.DeserializeObject(data.GetValue("PipelineInfo").ToString());
                var pipelineType = pipelineInfoObject.GetJsonValue<PipelineType>("PipelineType");

                PipelineConfiguration pipelineConfiguration = null;
                if (pipelineType == PipelineType.Release)
                {
                    pipelineConfiguration = new ReleaseConfiguration()
                    {
                        ProjectId = pipelineInfoObject.GetJsonValue<string>("ProjectId"),
                        ProjectName = pipelineInfoObject.GetJsonValue<string>("ProjectName"),
                        ReleaseId = pipelineInfoObject.GetJsonValue<int>("Id"),
                        ServerUri = pipelineInfoObject.GetJsonValue<string>("ServerUri").ToString(),
                        RMServerUri = pipelineInfoObject.GetJsonValue<string>("RMServerUri").ToString(),
                        DefinitionEnvironmentId = pipelineInfoObject.GetJsonValue<int>("DefinitionEnvironmentId"),
                        EnvironmentId = pipelineInfoObject.GetJsonValue<int>("EnvironmentId"),
                        Credentials = credentials
                    };
                }
                else
                {
                    // TODO
                    return null;
                }

                return new EmailReportConfiguration()
                {
                    PipelineType = pipelineType,
                    ReportDataConfiguration = reportDataConfiguration,
                    MailConfiguration = mailConfiguration,
                    PipelineConfiguration = pipelineConfiguration
                };
            }

            return null;
        }
    }

    public static class JsonExtensionMethods
    {

        public static T GetJsonValue<T>(this JObject jObject, string key)
        {
            var jToken = jObject.GetValueOrDefault(key);
            return jToken == null ? default(T) : jToken.ToObject<T>();
        }
    }
}
