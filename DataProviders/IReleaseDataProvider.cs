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
        Task<List<ChangeData>> GetAssociatedChanges(Release lastCompletedRelease);

        Task<Release> GetReleaseByLastCompletedEnvironment(Release release, ReleaseEnvironment environment);

        Task<List<PhaseData>> GetPhases(ReleaseEnvironment environment);

    }
}