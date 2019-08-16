using Microsoft.TeamFoundation.TestManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.TestResults
{
    public interface ITestResultDetailsParser
    {
        List<TestSummaryItem> GetSummaryItems();

        string GetGroupByValue(TestResultsDetailsForGroup group);
    }
}
