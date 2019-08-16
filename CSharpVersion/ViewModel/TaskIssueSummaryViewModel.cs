using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.Utils;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;

namespace EmailReportFunction.ViewModel
{
    [DataContract]
    public class TaskIssueSummaryViewModel
    {
        [DataMember]
        public List<TaskIssueViewModel> Issues { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public int ErrorCount { get; set; }

        [DataMember]
        public int WarningCount { get; set; }

        public TaskIssueSummaryViewModel(List<TaskData> tasks)
        {
            var allIssues = new List<TaskIssueViewModel>();
            ErrorMessage = $"Failed on {tasks.Count(t => t.Status == TaskStatus.Failed || t.Status == TaskStatus.Canceled)}/{tasks.Count} Agents";
            foreach (var task in tasks)
            {
                if (task.Issues?.Any() != true)
                {
                    continue;
                }

                foreach (var issue in task.Issues)
                {
                    if (!string.IsNullOrWhiteSpace(issue.Message))
                    {
                        if (issue.IssueType.CompareIgnoreCase(IssueTypeConstants.Error))
                        {
                            ErrorCount++;
                        }
                        else if (issue.IssueType.CompareIgnoreCase(IssueTypeConstants.Warning))
                        {
                            WarningCount++;
                        }

                        allIssues.Add(new TaskIssueViewModel(issue.Message, issue.IssueType, task.AgentName));
                    }
                }
            }
            Issues = TruncateIssues(allIssues);
        }

        public List<TaskIssueViewModel> TruncateIssues(List<TaskIssueViewModel> issues, int characterLimit = 1000)
        {
            List<TaskIssueViewModel> truncatedIssues = new List<TaskIssueViewModel>();
            var sortedIssues = issues.OrderBy(t => !t.IssueType.Equals("Error")).ToList();

            int currentCharCount = 0;
            foreach (var issue in sortedIssues)
            {
                if (currentCharCount >= characterLimit)
                {
                    return truncatedIssues;
                }

                issue.Message = issue.Message.Truncate(characterLimit - currentCharCount);
                currentCharCount += issue.Message.Length;
                truncatedIssues.Add(issue);
            }

            return truncatedIssues;
        }
    }

    public static class IssueTypeConstants
    {
        public const string Error = "Error";
        public const string Warning = "Warning";
    }
}
