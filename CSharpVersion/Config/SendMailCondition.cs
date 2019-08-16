using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config
{
    public enum SendMailCondition
    {
        Always = 0,
        OnFailure,
        OnSuccess,
        OnNewFailuresOnly
    }
}
