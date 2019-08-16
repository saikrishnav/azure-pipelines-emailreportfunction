using EmailReportFunction.Config.TestResults;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailReportFunction.Utils
{
    public static class TestSummaryItemExtensions
    {
        public static int GetFailedTestsCount(this TestSummaryItem dto)
        {
            return GetTestOutcomeCount(dto, TestOutcome.Failed);
        }

        public static int GetOtherTestsCount(this TestSummaryItem dto)
        {
            return
                dto.TestCountByOutCome.Where(
                    kvPair => kvPair.Key != TestOutcome.Passed && kvPair.Key != TestOutcome.Failed)
                    .Select(kvPair => kvPair.Value)
                    .Sum();
        }

        public static int GetPassedTestsCount(this TestSummaryItem dto)
        {
            return GetTestOutcomeCount(dto, TestOutcome.Passed);
        }

        private static int GetTestOutcomeCount(TestSummaryItem dto, TestOutcome testOutcome)
        {
            if (dto.TestCountByOutCome.ContainsKey(testOutcome))
            {
                return dto.TestCountByOutCome[testOutcome];
            }
            return 0;
        }
    }
}
