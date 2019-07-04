using EmailReportFunction.Config;
using EmailReportFunction.DataProviders;
using Microsoft.Extensions.Logging;

namespace EmailReportFunction.Wrappers
{
    public class ReportFactory : IReportFactory
    {
        private EmailReportConfiguration _emailReportConfiguration;
        private ILogger _logger;

        public ReportFactory(EmailReportConfiguration emailReportConfig, ILogger logger)
        {
            _emailReportConfiguration = emailReportConfig;
            DataProviderFactory = new DataProviderFactory(emailReportConfig, logger);
            _logger = logger;
        }

        public IDataProviderFactory DataProviderFactory { get; private set; }

        private IReportMessageGenerator _reportMessageGenerator;
        public IReportMessageGenerator ReportMessageGenerator
        {
            get
            {
                if (_reportMessageGenerator == null)
                {
                    _reportMessageGenerator = new ReportMessageGenerator(_emailReportConfiguration, this.DataProviderFactory, _logger);
                }
                return _reportMessageGenerator;
            }
        }

        private IMailSender _mailSender;
        public IMailSender MailSender
        {
            get
            {
                if (_mailSender == null)
                {
                    _mailSender = new MailSender(DataProviderFactory.GetDataProvider<SmtpConfiguration>(), _logger);
                }
                return _mailSender;
            }
        }
    }
}
