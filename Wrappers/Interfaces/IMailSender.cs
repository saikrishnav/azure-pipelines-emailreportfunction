using EmailReportFunction.Report;
using System.Threading.Tasks;

namespace EmailReportFunction.Wrappers
{
    public interface IMailSender
    {
        Task<bool> SendMailAsync(ReportMessage message);
    }
}