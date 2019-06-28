using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.Pipeline
{
    public class JobData
    {
        public List<TaskData> Tasks { get; set; }

        public TaskStatus JobStatus { get; set; }

        public List<IssueData> Issues { get; set; }

        public string JobName { get; set; }
    }
}
