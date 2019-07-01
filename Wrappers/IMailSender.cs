using EmailReportFunction.Config;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailReportFunction.Wrappers
{
    public interface IMailSender
    {
        Task<bool> SendMailAsync(MailMessage message);
    }
}