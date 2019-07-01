using System;
using System.Collections.Generic;
using EmailReportFunction.Config.TestResults;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace EmailReportFunction.ViewModel.Helpers
{
    public static class TestResultsHelper
    {
        public const int PercentagePrecision = 2;

        public static double GetTestOutcomePercentage(int testCountForOutcome, int totalTests)
        {
            if (totalTests == 0)
            {
                //TODO - Log.LogInfo("Total Test count is 0. Setting outcome percentage to 0");
            }

            double testOutcomePercentage = totalTests == 0 ?
                0 : 
                (double) testCountForOutcome / totalTests * 100;

            return GetCustomizedDecimalValue(testOutcomePercentage);
        }

        private static double GetCustomizedDecimalValue(double value)
        {
            var fixedValue = Math.Pow(10, PercentagePrecision);
            return ((Math.Floor(value * fixedValue)) / fixedValue);
        }


        public static string GetTestOutcomePercentageString(int testCountForOutcome, int totalTests)
        {
            return GetTestOutcomePercentage(testCountForOutcome, totalTests) + "%";
        }

        public static int GetTotalTestCountBasedOnUserConfiguration(
            Dictionary<TestOutcome, int> testCountsByOutcome,
            bool includeOthersInTotal)
        {
            var totalTests = 0;

            foreach (KeyValuePair<TestOutcome, int> testKvPair in testCountsByOutcome)
            {
                TestOutcome testOutcome = testKvPair.Key;
                var testCount = testKvPair.Value;

                var isPassedTest = testOutcome == TestOutcome.Passed;
                var isFailedTest = testOutcome == TestOutcome.Failed;

                if (isPassedTest || isFailedTest || includeOthersInTotal)
                {
                    totalTests += testCount;
                }
            }

            return totalTests;
        }

        public static int GetTotalTestCountBasedOnUserConfiguration(
            Dictionary<TestOutcomeForPriority, int> testCountsByOutcome, bool includeOthersInTotal)
        {
            var totalTests = 0;

            foreach (KeyValuePair<TestOutcomeForPriority, int> testKvPair in testCountsByOutcome)
            {
                TestOutcomeForPriority testOutcome = testKvPair.Key;
                var testCount = testKvPair.Value;

                var isPassedTest = testOutcome == TestOutcomeForPriority.Passed;
                var isFailedTest = testOutcome == TestOutcomeForPriority.Failed;

                if (isPassedTest || isFailedTest || includeOthersInTotal)
                {
                    totalTests += testCount;
                }
            }

            return totalTests;
        }
    }
}