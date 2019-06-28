using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.DataProviders;
using EmailReportFunction.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.OAuth;
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
            IPipelineData data = await GatherReportDataAsync(jsonRequest, logger);
            await SendReportEmailAsync(data, logger);
            return true;
        }

        #region Private Methods

        private async Task<IPipelineData> GatherReportDataAsync(string jsonRequest, ILogger log)
        {
            var dataProvider = _reportFactory.GetDataProvider(jsonRequest, log);
            return await dataProvider.GetPipelineData();
        }

        private async Task<bool> SendReportEmailAsync(IPipelineData data, ILogger logger)
        {
            var mailSender = _reportFactory.GetMailSender(logger);
            return await mailSender.SendMailAsync(data);
        }

        #endregion
    }
}