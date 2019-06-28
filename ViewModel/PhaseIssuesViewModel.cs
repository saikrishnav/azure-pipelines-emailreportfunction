using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.EmailTask.EmailReport.Dto;

namespace Microsoft.EmailTask.EmailReport.ViewModel
{
    [DataContract]
    public class PhaseIssuesViewModel
    {
        /// <summary>
        /// Use TaskResultViewModel as Phase level issue as the viewmodel is same
        /// </summary>
        [DataMember]
        public List<TaskResultViewModel> Tasks { get; set; }

        [DataMember]
        public string Name { get; set; }

        public PhaseIssuesViewModel()
        {
            Tasks = new List<TaskResultViewModel>();
        }

        public PhaseIssuesViewModel(List<PhaseDto> phases)
        {
            Name = "Phase Issues";
            Tasks = new List<TaskResultViewModel>();
            foreach (var phase in phases)
            {
                if (phase != null && phase.Jobs != null)
                {
                    var canceledJobs = phase.Jobs.Where(job => job.JobStatus == TaskStatus.Canceled);
                    if (canceledJobs.Any())
                    {
                        var failedJobsAsTasks = canceledJobs.Select(job => new TaskDto() { Name = job.JobName, Issues = job.Issues, Status = job.JobStatus });
                        var taskResViewModel = new TaskResultViewModel(failedJobsAsTasks.ToList());
                        taskResViewModel.IssuesSummary.ErrorMessage = $"Failed on {canceledJobs.Count()}/{phase.Jobs.Count} Agents";
                        Tasks.Add(taskResViewModel);
                    }
                }
            }
        }
    }
}
