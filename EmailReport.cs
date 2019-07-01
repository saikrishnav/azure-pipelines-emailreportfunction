using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.DataProviders;
using EmailReportFunction.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction
{
    public class EmailReport
    {
        private IReportFactory _reportFactory;

        public EmailReport(IReportFactory reportFactory)
        {
            this._reportFactory = reportFactory;
        }

        public async Task<bool> GenerateAndSendReport(string jsonRequest, ILogger logger)
        {
            var dataProvider = _reportFactory.GetPipelineDataProvider(jsonRequest, logger);
            var pipelineData = await dataProvider.GetDataAsync();

            var reportGenerator = _reportFactory.GetReportMessageGenerator(jsonRequest, logger);
            var message = await reportGenerator.GenerateReportAsync(pipelineData);

            var mailSender = _reportFactory.GetMailSender(jsonRequest, logger);
            return await mailSender.SendMailAsync(message);
        }
    }
}