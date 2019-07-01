using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.ViewModel.Helpers;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;

namespace EmailReportFunction.ViewModel
{
    [DataContract]
    public class TaskResultViewModel
    {
        [DataMember]
        public string Duration { get; set; }

        [DataMember]
        public bool HasFailed { get; set; }

        [DataMember]
        public bool HasNotRunOnSomeAgents { get; set; }

        [DataMember]
        public string NotRunMessage { get; set; }

        [DataMember]
        public TaskIssueSummaryViewModel IssuesSummary { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string StartTime { get; set; }

        [DataMember]
        public string Status { get; set; }

        public TaskResultViewModel()
        {
        }

        public TaskResultViewModel(List<TaskData> tasks)
        {
            Name = tasks.First().Name;

            HasFailed = tasks.Any(t => t.Status == TaskStatus.Failed || t.Status == TaskStatus.Canceled);

            if (tasks.Count(t => t.Status == TaskStatus.Skipped) == tasks.Count)
            {
                HasFailed = true;
            }

            if (tasks.Count > 1)
            {
                HasNotRunOnSomeAgents = tasks.Any(t => t.Status == TaskStatus.Skipped);
                NotRunMessage = $"Not run on {tasks.Count(t => t.Status == TaskStatus.Skipped)}/{tasks.Count} agents";
            }

            IssuesSummary = new TaskIssueSummaryViewModel(tasks);

            InitializeDuration(tasks);
        }

        private void InitializeDuration(List<TaskData> tasks)
        {
            if (tasks.Count == 1)
            {
                var firstTask = tasks.First();
                if (firstTask.FinishTime.HasValue && firstTask.StartTime.HasValue)
                {
                    Duration = TimeSpanFormatter.FormatDuration(firstTask.FinishTime.Value.Subtract(firstTask.StartTime.Value),
                            true);
                }
            }
            else if(tasks.Any(t => t.FinishTime.HasValue && t.StartTime.HasValue))
            {
                var minTime = tasks.Where(t => t.FinishTime.HasValue && t.StartTime.HasValue).Min(t => t.FinishTime.Value.Subtract(t.StartTime.Value));
                var maxTime = tasks.Where(t => t.FinishTime.HasValue && t.StartTime.HasValue).Max(t => t.FinishTime.Value.Subtract(t.StartTime.Value));

                if (minTime != null && maxTime != null)
                {
                    Duration = $"{TimeSpanFormatter.FormatDuration(minTime, false)} - {TimeSpanFormatter.FormatDuration(maxTime, false)}";
                }
            }
        }
    }
}