using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EmailReportFunction.Config.TestResults
{
    public enum TestResultsGroupingType
    {

        [Description("Priority")]
        Priority = 0,
        [Description("Test Run")]
        Run,
        [Description("Team")]
        Team
    }
}
