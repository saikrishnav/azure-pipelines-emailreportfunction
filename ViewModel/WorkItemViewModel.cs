using System.Runtime.Serialization;
using Microsoft.EmailTask.EmailReport.Config;
using Microsoft.EmailTask.EmailReport.Utils;
using Microsoft.TeamFoundation.WorkItemTracking.Common;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace Microsoft.EmailTask.EmailReport.ViewModel
{
    [DataContract]
    public class WorkItemViewModel
    {
        [DataMember]
        public string AssignedTo { get; set; }

        [DataMember]
        public string ChangedDate { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string State { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Url { get; set; }

        public WorkItemViewModel()
        {
        }

        public WorkItemViewModel(BaseConfiguration config, WorkItem workItem)
        {
            if (workItem.Id.HasValue)
            {
                Id = workItem.Id.Value;
                Url = LinkHelper.GetWorkItemLink(config, workItem.Id.Value);
            }

            Title = workItem.GetWorkItemField<string>(DatabaseCoreFieldRefName.Title);

            // This is for display in email report only
            var assignToRef = workItem.GetWorkItemField<IdentityRef>(DatabaseCoreFieldRefName.AssignedTo);
            // Prefer Display name to Unique Name in report
            AssignedTo = assignToRef == null ? string.Empty : (string.IsNullOrEmpty(assignToRef.DisplayName) ? assignToRef.UniqueName : assignToRef.DisplayName);

            State = workItem.GetWorkItemField<string>(DatabaseCoreFieldRefName.State);
            ChangedDate = workItem.GetWorkItemField<string>(DatabaseCoreFieldRefName.ChangedDate);
        }
    }
}