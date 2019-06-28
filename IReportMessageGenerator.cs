using EmailReportFunction.Config.Pipeline;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace EmailReportFunction
{
    public interface IReportMessageGenerator
    {
        MailMessage GenerateReport(IPipelineData pipelineData);
    }
}
