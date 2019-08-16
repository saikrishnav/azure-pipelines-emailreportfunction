using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.Wrappers
{
    public interface IWorkItemTrackingApiHelper
    {
        Task<List<WorkItem>> GetWorkItemsAsync(IEnumerable<int> ids, IEnumerable<string> fields = null);
    }
}
