using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.Wrappers;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.Report
{
    public interface IReportMessageCreator
    {
        Task<ReportMessage> CreateMessageAsync(AbstractReport reportData);
    }
}
