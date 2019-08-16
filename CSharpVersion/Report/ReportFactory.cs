using EmailReportFunction.Config;
using EmailReportFunction.DataProviders;
using EmailReportFunction.PostProcessor;
using EmailReportFunction.Wrappers;
using Microsoft.Extensions.Logging;

namespace EmailReportFunction.Report
{
    public class ReportFactory : IReportFactory
    {
        private EmailReportConfiguration _emailReportConfiguration;
        private ILogger _logger;

        public ReportFactory(EmailReportConfiguration emailReportConfig, ILogger logger)
        {
            _emailReportConfiguration = emailReportConfig;
            _dataProviderFactory = new DataProviderFactory(emailReportConfig, logger);
            _logger = logger;
        }

        private IDataProviderFactory _dataProviderFactory;

        private IReportMessageCreator _reportMessageCreator;
        public IReportMessageCreator MessageCreator
        {
            get
            {
                if (_reportMessageCreator == null)
                {
                    _reportMessageCreator = new ReportMessageCreator(_emailReportConfiguration, _logger);
                }
                return _reportMessageCreator;
            }
        }

        private IReportGenerator _reportGenerator;
        public IReportGenerator ReportGenerator
        {
            get
            {
                if (_reportGenerator == null)
                {
                    _reportGenerator = new ReportGenerator(_dataProviderFactory, _emailReportConfiguration.PipelineType, _logger);
                }
                return _reportGenerator;
            }
        }

        private IMailSender _mailSender;
        public IMailSender MailSender
        {
            get
            {
                if (_mailSender == null)
                {
                    _mailSender = new MailSender(_logger);
                }
                return _mailSender;
            }
        }
    }
}