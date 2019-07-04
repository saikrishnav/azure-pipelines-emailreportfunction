using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmailReportFunction.DataProviders
{
    /// <summary>
    /// Data Providers provide data to fill the report
    /// </summary>
    public interface IReleaseDataProvider : IDataProvider<IPipelineData>
    {
        Task<List<ChangeData>> GetAssociatedChangesAsync(Release lastCompletedRelease);

        Task<Release> GetReleaseByLastCompletedEnvironmentAsync(Release release, ReleaseEnvironment environment);

        Task<List<PhaseData>> GetPhasesAsync(ReleaseEnvironment environment);

    }
}