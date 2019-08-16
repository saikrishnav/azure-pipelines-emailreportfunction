using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
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

            JToken executeConditionObject;
            if (data.TryGetValue("ExecuteCondition", out executeConditionObject) && executeConditionObject != null)
            {
                var executeCondition = JsonConvert.DeserializeObject<ExecuteCondition>(executeConditionObject.ToString());
                // Evaluate condition and if fails, exit
                if(!executeCondition.Evaluate())
                {
                    return null;
                }
            }

            StringValues authTokenValues;
            headers.TryGetValue("AuthToken", out authTokenValues);
            var accessToken = authTokenValues.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var credentials = new VssBasicCredential("", accessToken);
                var reportDataConfiguration = JsonConvert.DeserializeObject<ReportDataConfiguration>(data.GetValue("ReportDataConfiguration").ToString());
                var mailConfiguration = JsonConvert.DeserializeObject<MailConfiguration[]>(data.GetValue("EmailConfiguration").ToString());

                var pipelineInfoObject = (JObject)JsonConvert.DeserializeObject(data.GetValue("PipelineInfo").ToString());
                var pipelineType = pipelineInfoObject.GetValue("PipelineType").ToObject<PipelineType>();

                PipelineConfiguration pipelineConfiguration = null;
                if (pipelineType == PipelineType.Release)
                {
                    pipelineConfiguration = JsonConvert.DeserializeObject<ReleaseConfiguration>(data.GetValue("PipelineInfo").ToString());
                    pipelineConfiguration.Credentials = credentials;
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
                    MailConfigurations = mailConfiguration,
                    PipelineConfiguration = pipelineConfiguration
                };
            }

            return null;
        }
    }
}
