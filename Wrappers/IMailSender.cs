using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EmailReportFunction.Wrappers
{
    public interface IMailSender
    {
        Task<bool> SendMailAsync(IPipelineData pipelineData);
    }
}