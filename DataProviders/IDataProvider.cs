using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EmailReportFunction.DataProviders
{
    /// <summary>
    /// Data Providers provide data to fill the report
    /// </summary>
    public interface IDataProvider
    {
        Task AddReportDataAsync(AbstractReport reportData);
    }
}