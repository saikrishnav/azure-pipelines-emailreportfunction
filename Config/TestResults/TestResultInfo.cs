using Microsoft.TeamFoundation.TestManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.TestResults
{
    public class TestResultInfo
    {
        public TestResultInfo()
        {
            TestAttachments = new List<TestAttachment>();
        }

        public int TestResultId { get; set; }

        public int TestRunId { get; set; }

        public string AutomatedTestName { get; set; }

        public string Outcome { get; set; }

        public int Priority { get; set; }

        public int TestCaseReferenceId { get; set; }

        public string Owner { get; set; }

        public string StackTrace { get; set; }

        public string ErrorMessage { get; set; }

        public bool PassedOnRerun { get; set; }

        public List<TestAttachment> TestAttachments { get; }
    }
}
