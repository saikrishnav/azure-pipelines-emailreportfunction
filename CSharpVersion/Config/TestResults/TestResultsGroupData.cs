using Microsoft.TeamFoundation.TestManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.TestResults
{
    public class TestResultsGroupData
    {
        public string GroupName { get; set; }

        public Dictionary<TestOutcome, List<TestResultData>> TestResults { get; set; } = new Dictionary<TestOutcome, List<TestResultData>>();
    }
}
