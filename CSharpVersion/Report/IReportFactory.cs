using EmailReportFunction.Wrappers;

namespace EmailReportFunction.Report
{
    public interface IReportFactory
    {
        IReportMessageCreator MessageCreator { get; }

        IMailSender MailSender { get; }

        IReportGenerator ReportGenerator { get; }
    }
}