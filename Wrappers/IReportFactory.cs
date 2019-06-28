﻿using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.DataProviders;
using EmailReportFunction.Wrappers.Microsoft.EmailTask.EmailReport.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;

namespace EmailReportFunction.Wrappers
{
    public interface IReportFactory
    {
        IMailSender GetMailSender(ILogger logger);

        IDataProvider<IPipelineData> GetPipelineDataProvider(string jsonRequest, ILogger logger);
    }
}