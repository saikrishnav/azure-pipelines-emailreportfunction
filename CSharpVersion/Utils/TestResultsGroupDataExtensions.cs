using EmailReportFunction.Config.TestResults;
using EmailReportFunction.Exceptions;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailReportFunction.Utils
{
    public static class TestResultsGroupDataExtensions
    {
        public static void AddTestResult(this TestResultsGroupData source, TestResultData result)
        {
            if (!string.IsNullOrEmpty(result.TestResult.Outcome))
            {
                var testOutcome = EnumHelper.Parse<TestOutcome>(result.TestResult.Outcome);
                if (!source.TestResults.ContainsKey(testOutcome))
                {
                    source.TestResults[testOutcome] = new List<TestResultData>();
                }

                source.TestResults[testOutcome].Add(result);
            }
            else
            {
                // TODO - Log.LogWarning(
                    //"Found test with outcome as null. " +
                    //$"Test result id {result.TestResult.Id} in Test run {result.TestResult.TestRun?.Id}");
            }
        }

        public static List<TestResultData> GetTestResultsByOutcomes(this TestResultsGroupData source,
            params TestOutcome[] outcomes)
        {
            var testResults = new List<TestResultData>();

            foreach (TestOutcome outcome in outcomes)
            {
                if (source.TestResults.ContainsKey(outcome))
                {
                    testResults.AddRange(source.TestResults[outcome]);
                }
            }
            return testResults;
        }

        public static bool HasFilteredTestResults(this TestResultsDetails resultIdsToFetch, int maxItems)
        {
            var hasFiltered = false;
            var remainingItems = maxItems;
            foreach (TestResultsDetailsForGroup group in resultIdsToFetch.ResultsForGroup)
            {
                var currentItemsSize = group.Results.Count;
                if (currentItemsSize > remainingItems)
                {
                    hasFiltered = true;
                    List<TestCaseResult> results = group.Results.ToList();
                    var itemCountToRemove = currentItemsSize - remainingItems;
                    results.RemoveLastNItems(itemCountToRemove);
                    group.Results = results;
                }
                remainingItems -= group.Results.Count;
            }

            resultIdsToFetch.ResultsForGroup =
                resultIdsToFetch.ResultsForGroup.Where(group => group.Results.Any()).ToList();
            return hasFiltered;
        }

        public static void RemoveLastNItems<TSource>(this List<TSource> source, int n)
        {
            if (n < 0 || n > source.Count)
            {
                throw new EmailReportException($"Item count to remove cannot be less than zer or greater than list size. Passed value - {n}");
            }

            if (n == source.Count)
            {
                source.Clear();
            }
            else
            {
                source.RemoveRange(source.Count - n, n);
            }
        }
    }
}
