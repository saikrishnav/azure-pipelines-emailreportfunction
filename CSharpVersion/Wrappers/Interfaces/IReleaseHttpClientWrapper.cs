using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;

namespace EmailReportFunction.Wrappers
{
    namespace Microsoft.EmailTask.EmailReport.Wrappers
    {
        public interface IReleaseHttpClientWrapper
        {
            Task<List<Change>> GetReleaseChangesAsync(
                string project,
                int releaseId,
                int? baseReleaseId = null);

            Task<Release> GetReleaseAsync(
                string project,
                int releaseId);

            Task<List<Release>> GetReleasesAsync(
                string project,
                int? definitionId = null,
                int? definitionEnvironmentId = null,
                int? environmentStatusFilter = null,
                ReleaseQueryOrder? queryOrder = null,
                int? top = null,
                ReleaseExpands? expand = null,
                string artifactId = null,
                string sourceBranchFilter = null);
        }
    }
}
