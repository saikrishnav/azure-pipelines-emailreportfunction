using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.TestResults
{
    public class TestResultsConstants
    {
        public const string TestRun = "TestRun";
        public const string Priority = "Priority";

        private static readonly Dictionary<TestResultsGroupingType, string> EnumToConstantName = new Dictionary<TestResultsGroupingType, string>
        {
            {TestResultsGroupingType.Run, TestRun},
            {TestResultsGroupingType.Priority, Priority}
        };

        public static string GetName(TestResultsGroupingType groupBy)
        {
            if (EnumToConstantName.ContainsKey(groupBy))
            {
                return EnumToConstantName[groupBy];
            }
            //Log.LogWarning(
            //    $"Test result group by - {groupBy} not Supported. " +
            //    $"Using {TestRun} group");
            return TestRun;
        }
    }
}
