using EmailReportFunction.Wrappers.Microsoft.EmailTask.EmailReport.Wrappers;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;

namespace EmailReportFunction.Wrappers
{
    internal class ReleaseHttpClientWrapper : IReleaseHttpClientWrapper
    {
        private readonly ReleaseHttpClient _client;

        public ReleaseHttpClientWrapper(ReleaseHttpClient client)
        {
            _client = client;
        }

        public Task<List<Change>> GetReleaseChangesAsync(
            string project,
            int releaseId,
            int? baseReleaseId = null)
        {
            return _client.GetReleaseChangesAsync(project, releaseId, baseReleaseId, 200);
        }

        public Task<Release> GetReleaseAsync(
            string project,
            int releaseId)
        {
            return _client.GetReleaseAsync(project, releaseId);
        }

        public Task<List<Release>> GetReleasesAsync(
            string project,
            int? definitionId = null,
            int? definitionEnvironmentId = null,
            int? environmentStatusFilter = null,
            ReleaseQueryOrder? queryOrder = null,
            int? top = null,
            ReleaseExpands? expand = null,
            string artifactId = null,
            string sourceBranchFilter = null)
        {
            return _client.GetReleasesAsync(project, definitionId,
                definitionEnvironmentId, null, null, null,
                environmentStatusFilter,
                null, null, queryOrder, top, null, expand, null, artifactId, null, sourceBranchFilter);
        }
    }
}
