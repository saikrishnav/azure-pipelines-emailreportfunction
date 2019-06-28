using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.Pipeline
{
    public class TaskData
    {
        public string Name { get; set; }

        public TaskStatus Status { get; set; }

        public List<IssueData> Issues { get; set; }

        public string AgentName { get; set; }

        public DateTime? FinishTime { get; set; }

        public DateTime? StartTime { get; set; }
    }
}
