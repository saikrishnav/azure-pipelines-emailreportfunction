using System.Runtime.Serialization;
using EmailReportFunction.Config;
using EmailReportFunction.Config.WIT;
using EmailReportFunction.Utils;
using EmailReportFunction.ViewModel.Helpers;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace EmailReportFunction.ViewModel
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

        public WorkItemViewModel(PipelineConfiguration config, WorkItem workItem)
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