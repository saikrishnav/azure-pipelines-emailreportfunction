using EmailReportFunction.DataProviders;

namespace EmailReportFunction.Wrappers
{
    public interface IReportFactory
    {
        IDataProviderFactory DataProviderFactory { get; }

        IReportMessageGenerator ReportMessageGenerator { get; }

        IMailSender MailSender { get; }
    }
}