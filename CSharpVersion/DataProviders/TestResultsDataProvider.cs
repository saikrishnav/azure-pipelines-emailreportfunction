using EmailReportFunction.Config;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.DataProviders;
using EmailReportFunction.Exceptions;
using EmailReportFunction.Utils;
using EmailReportFunction.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkItemReference = Microsoft.TeamFoundation.TestManagement.WebApi.WorkItemReference;

namespace EmailReportFunction.DataProviders
{
    public class TestResultsDataProvider : IDataProvider
    {
        private readonly ITcmApiHelper _tcmApiHelper;
        private readonly IWorkItemTrackingApiHelper _workItemTrackingApiHelper;
        private readonly ReportDataConfiguration _reportDataConfiguration;
        private readonly ILogger _logger;

        public TestResultsDataProvider(ITcmApiHelper tcmApiHelper,
            IWorkItemTrackingApiHelper workItemTrackingApiHelper,
            ReportDataConfiguration reportDataConfiguration,
            ILogger logger)
        {
            _logger = logger;
            _tcmApiHelper = tcmApiHelper;
            _workItemTrackingApiHelper = workItemTrackingApiHelper;
            _reportDataConfiguration = reportDataConfiguration;
        }

        public async Task AddReportDataAsync(AbstractReport reportData)
        {
            using (new PerformanceMeasurementBlock("In TestResultsDataProvider", _logger))
            {
                // This is to make sure the failing since information is computed before we fetch test results
                await _tcmApiHelper.QueryTestResultsReportAsync();
                _logger.LogInformation("Fetched test results data");
                await GetFilteredTestResultsAsync(reportData);
            }
        }

        #region Helper Methods

        private List<TestOutcome> GetIncludedOutcomes()
        {
            var includedOutcomes = new List<TestOutcome>();
            if (_reportDataConfiguration.IncludeFailedTests)
            {
                includedOutcomes.Add(TestOutcome.Failed);
            }

            if (_reportDataConfiguration.IncludeOtherTests)
            {
                includedOutcomes.AddRange(EnumHelper.GetEnumsExcept(TestOutcome.Failed, TestOutcome.Passed));
            }

            if (_reportDataConfiguration.IncludePassedTests)
            {
                includedOutcomes.Add(TestOutcome.Passed);
            }

            return includedOutcomes;
        }

        private async Task GetFilteredTestResultsAsync(AbstractReport reportData)
        {
            if (_reportDataConfiguration.IncludeFailedTests || _reportDataConfiguration.IncludeOtherTests ||
                _reportDataConfiguration.IncludePassedTests)
            {
                var groupBy = TestResultsConstants.GetName(_reportDataConfiguration.GroupTestResultsBy);
                var includedOutcomes = GetIncludedOutcomes();

                var resultIdsToFetch = await _tcmApiHelper.GetTestSummaryAsync(groupBy, includedOutcomes.ToArray());
                reportData.HasFilteredTests = FilterTestResults(resultIdsToFetch, _reportDataConfiguration.MaxFailuresToShow);
                reportData.FilteredResults = (await GetTestResultsWithWorkItemsAsync(resultIdsToFetch)).ToList();
            }
        }

        private async Task<TestResultsGroupData[]> GetTestResultsWithWorkItemsAsync(TestResultsDetails resultIdsToFetch)
        {
            TestResultsDetailsForGroup[] resultDetailGroupsToParse =
                resultIdsToFetch.ResultsForGroup.ToArray();

            var testResultDetailsParser = GetParser(resultIdsToFetch);

            var filteredTestResultGroups = await resultIdsToFetch
                .ResultsForGroup
                .ParallelSelectAsync(async resultsForGroup =>
                {
                    var resultGroup = new TestResultsGroupData
                    {
                        GroupName = testResultDetailsParser.GetGroupByValue(resultsForGroup)
                    };

                    var results = await GetTestResultsWithBugRefsAsync(resultsForGroup);
                    var bugsRefs = results.Select(result => result.AssociatedBugRefs).Merge();

                    var workItemDictionary = await GetWorkItemsAsync(bugsRefs);

                    SetAssociatedBugs(results, workItemDictionary);
                    results.ForEach(resultGroup.AddTestResult);
                    return resultGroup;
                });

            return filteredTestResultGroups;
        }

        private bool FilterTestResults(TestResultsDetails resultIdsToFetch, int maxItems)
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

        private async Task<TestResultData[]> GetTestResultsWithBugRefsAsync(TestResultsDetailsForGroup resultsForGroup)
        {
            var results = await resultsForGroup.Results
                .ParallelSelectAsync(async resultIdObj =>
                {
                    var resultData = new TestResultData();

                    resultData.TestResult =
                        await _tcmApiHelper.GetTestResultByIdAsync(int.Parse(resultIdObj.TestRun.Id), resultIdObj.Id);

                        // Remove flaky tests
                        if (resultData.TestResult.IsTestFlaky())
                    {
                        return null;
                    }

                    resultData.AssociatedBugRefs =
                        await _tcmApiHelper.QueryTestResultBugsAsync(
                            resultData.TestResult.AutomatedTestName,
                            resultData.TestResult.Id);

                    return resultData;
                });

            //Remove all null values from array
            results = results.Where(r => r != null).ToArray();
            return results;
        }

        private static void SetAssociatedBugs(TestResultData[] results, Dictionary<int, WorkItem> workItemDictionary)
        {
            foreach (var resultDto in results)
            {
                resultDto.AssociatedBugs = new List<WorkItem>();
                if (resultDto.AssociatedBugRefs != null)
                {
                    foreach (WorkItemReference workItemReference in resultDto.AssociatedBugRefs)
                    {
                        resultDto.AssociatedBugs.Add(workItemDictionary[int.Parse(workItemReference.Id)]);
                    }
                }
            }
        }

        private async Task<Dictionary<int, WorkItem>> GetWorkItemsAsync(List<WorkItemReference> bugsRefs)
        {
            var workItemDictionary = new Dictionary<int, WorkItem>();

            if (bugsRefs.Any())
            {
                List<WorkItem> workItems = await _workItemTrackingApiHelper.GetWorkItemsAsync(bugsRefs.Select(bugRef => int.Parse(bugRef.Id)));
                foreach (WorkItem workItem in workItems)
                {
                    if (workItem.Id.HasValue)
                    {
                        workItemDictionary[workItem.Id.Value] = workItem;
                        continue;
                    }
                    _logger.LogWarning($"Unable to get id for a work item");
                }
            }

            return workItemDictionary;
        }

        private ITestResultDetailsParser GetParser(TestResultsDetails resultDetails)
        {
            var groupByField = resultDetails.GroupByField;
            if (string.Equals(groupByField, TestResultsConstants.TestRun, StringComparison.InvariantCultureIgnoreCase))
            {
                return new TestResultDetailsParserForRun(resultDetails, _logger);
            }

            if (string.Equals(groupByField, TestResultsConstants.Priority,
                StringComparison.InvariantCultureIgnoreCase))
            {
                return new TestResultDetailsParserForPriority(resultDetails, _logger);
            }

            throw new EmailReportException($"TestResultsDetails by group {groupByField} not supported");
        }

        #endregion
    }
}
