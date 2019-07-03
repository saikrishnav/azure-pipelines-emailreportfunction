using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;
using WorkItemReference = Microsoft.TeamFoundation.TestManagement.WebApi.WorkItemReference;

namespace EmailReportFunction.Config.TestResults
{
    public class TestResultData
    {
        public TestCaseResult TestResult { get; set; }
        public List<WorkItemReference> AssociatedBugRefs { get; set; }
        public List<WorkItem> AssociatedBugs { get; set; }
    }
}
