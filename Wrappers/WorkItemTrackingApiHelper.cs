using EmailReportFunction.Utils;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.Wrappers
{
    public class WorkItemTrackingApiHelper : IWorkItemTrackingApiHelper
    {
        internal const int MaxSupportedItemsPerQuery = 50;

        private WorkItemTrackingHttpClient _workItemTrackingHttpClient;

        public WorkItemTrackingApiHelper(WorkItemTrackingHttpClient workItemTrackingHttpClient)
        {
            _workItemTrackingHttpClient = workItemTrackingHttpClient;
        }

        public async Task<List<WorkItem>> GetWorkItemsAsync(IEnumerable<int> ids, IEnumerable<string> fields = null)
        {
            var workItemIds = ids.ToList();
            var chunks = workItemIds.Split(MaxSupportedItemsPerQuery).ToArray();
            var chunkResults = await chunks
                .ParallelSelectOnArrayAsync(async chunk => await _workItemTrackingHttpClient.GetWorkItemsAsync(chunk, fields));

            return chunkResults.Merge();
        }
    }
}
