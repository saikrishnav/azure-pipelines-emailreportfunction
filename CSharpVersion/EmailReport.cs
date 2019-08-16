using EmailReportFunction.Report;
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

        public async Task<bool> GenerateAndSendReport()
        {
            var mailSent = false;
            var report = await _reportFactory.ReportGenerator.FetchReportAsync();
            if (report.SendMailConditionSatisfied)
            {
                var message = await _reportFactory.MessageCreator.CreateMessageAsync(report);
                mailSent = await _reportFactory.MailSender.SendMailAsync(message);
            }
            return mailSent;
        }
    }
}