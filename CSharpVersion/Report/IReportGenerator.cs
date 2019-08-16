using EmailReportFunction.Config;
using System.Threading.Tasks;

namespace EmailReportFunction.Report
{
    public interface IReportGenerator
    {
        Task<AbstractReport> FetchReportAsync();
    }
}