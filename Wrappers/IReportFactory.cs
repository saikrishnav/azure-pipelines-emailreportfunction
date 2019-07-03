using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.DataProviders;

namespace EmailReportFunction.Wrappers
{
    public interface IReportFactory
    {
        IMailSender GetMailSender();

        IDataProvider<IPipelineData> GetPipelineDataProvider();

        IReportMessageGenerator ReportMessageGenerator { get; }
    }
}