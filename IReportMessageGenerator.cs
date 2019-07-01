using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction
{
    public interface IReportMessageGenerator
    {
        Task<MailMessage> GenerateReportAsync(IPipelineData pipelineData);
    }
}
