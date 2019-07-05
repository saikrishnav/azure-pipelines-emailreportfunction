using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.DataProviders;
using EmailReportFunction.Utils;
using EmailReportFunction.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace EmailReportFunction.PostProcessor
{
    internal class SendMailConditionPostProcessor : IDataPostProcessor
    {
        private readonly ITcmApiHelper _tcmApiHelper;

        private readonly EmailReportConfiguration _emailReportConfiguration;

        private readonly ILogger _logger;

        private readonly List<string> TestResultFieldsToQuery = new List<string>() { TestResultFieldNameConstants.TestCaseReferenceId, TestResultFieldNameConstants.OutcomeConfidence };

        public SendMailConditionPostProcessor(EmailReportConfiguration emailReportConfiguration, ITcmApiHelper tcmApiHelper, ILogger logger)
        {
            _tcmApiHelper = tcmApiHelper;
            _emailReportConfiguration = emailReportConfiguration;
            _logger = logger;
        }

        public async Task PostProcessAsync(ReportData emailReportDto)
        {
            var sendMailCondition = _emailReportConfiguration.MailConfiguration.SendMailCondition;

            var shouldSendMail = emailReportDto.DataMissing || sendMailCondition == SendMailCondition.Always;
            if (!shouldSendMail)
            {
                var hasTestFailures = emailReportDto.HasFailedTests(_emailReportConfiguration.ReportDataConfiguration.IncludeOthersInTotal);
                var hasFailedTasks = emailReportDto.HasFailedTasks();
                var hasCanceledPhases = emailReportDto.HasCanceledPhases();
                var hasFailure = hasTestFailures || hasFailedTasks || hasCanceledPhases;

                if ((sendMailCondition == SendMailCondition.OnFailure && hasFailure)
                    || (sendMailCondition == SendMailCondition.OnSuccess && !hasFailure))
                {
                    shouldSendMail = true;
                }
                else if (sendMailCondition == SendMailCondition.OnNewFailuresOnly && hasFailure)
                {
                    // Always treat phase cancellation issues as new failure as we cannot distinguish/compare phase level issues
                    // Still compare failed tests and failed tasks where possible to reduce noise
                    if (hasCanceledPhases && !hasTestFailures && !hasFailedTasks)
                    {
                        shouldSendMail = true;
                        _logger.LogInformation($"Has Phase cancellation(s) issues. Treating as new failure.");
                    }
                    else
                    {
                        _logger.LogInformation(
                        $"Looking for new failures, as the user send mail condition is '{sendMailCondition}'.");
                        var hasPreviousReleaseGotSameFailures = await HasPreviousReleaseGotSameFailuresAsync(emailReportDto, _emailReportConfiguration.PipelineConfiguration, hasTestFailures, hasFailedTasks);
                        shouldSendMail = !hasPreviousReleaseGotSameFailures;
                    }
                }
            }

            emailReportDto.SendMailConditionSatisfied = shouldSendMail;
        }

        public async Task<bool> HasPreviousReleaseGotSameFailuresAsync(ReportData emailReportDto, PipelineConfiguration config, bool hasTestFailures, bool hasFailedTasks)
        {
            var hasPrevGotSameFailures = emailReportDto.HasPrevGotSameFailures();
            if (hasPrevGotSameFailures.HasValue)
            {
                return hasPrevGotSameFailures.Value;
            }

            bool hasPrevFailedTasks = emailReportDto.HasPrevFailedTasks();
            if (emailReportDto.Summary == null)
            {
                return false;
            }

            if (hasTestFailures)
            {
                var prevConfig = emailReportDto.GetPrevConfig(config);
                var lastCompletedTestResultSummary = await _tcmApiHelper.QueryTestResultsReportAsync(prevConfig);

                var failedInCurrent = GetFailureCountFromSummary(emailReportDto.Summary);
                var failedinPrev = GetFailureCountFromSummary(lastCompletedTestResultSummary);

                // Threshold is 10 to decide whether they are same failures
                _logger.LogInformation($"Current Failures Found: '{failedInCurrent}'.");
                _logger.LogInformation($"Previous Failures Found: '{failedinPrev}'.");

                var hasSameFailures = failedInCurrent == failedinPrev;
                // No point in moving ahead if number of failures is different
                if (hasSameFailures)
                {
                    var currFailedTestCaseRefIds = await FetchFailedTestCaseIdsAsync(config);
                    var prevFailedTestCaseRefIds = await FetchFailedTestCaseIdsAsync(prevConfig);
                    var nonInteresection = currFailedTestCaseRefIds.Except(prevFailedTestCaseRefIds).Union(prevFailedTestCaseRefIds.Except(currFailedTestCaseRefIds));

                    if (nonInteresection.Any())
                    {
                        _logger.LogInformation($"Difference in Failed Test Reference Ids found between current - '{string.Join(",", nonInteresection.ToArray())}'.");
                        hasSameFailures = false;
                    }
                    else
                    {
                        _logger.LogInformation($"Failed Test Reference Ids match. No new failures found.");
                        hasSameFailures = true;
                    }
                }

                return hasSameFailures;
            }
            else if (hasFailedTasks && hasPrevFailedTasks)
            {
                return emailReportDto.ArePrevFailedTasksSame();
            }

            return false;
        }

        private static int GetFailureCountFromSummary(TestResultSummary testResultSummary)
        {
            return (testResultSummary.AggregatedResultsAnalysis.ResultsByOutcome.ContainsKey(TestOutcome.Failed))
                ? testResultSummary.AggregatedResultsAnalysis.ResultsByOutcome[TestOutcome.Failed].Count : 0;
        }

        protected virtual async Task<IEnumerable<int>> FetchFailedTestCaseIdsAsync(PipelineConfiguration pipelineConfiguration)
        {
            var testSummary = await _tcmApiHelper.GetTestSummaryAsync(pipelineConfiguration, null, TestOutcome.Failed);
            var resultsToQuery = testSummary.ResultsForGroup.SelectMany(rfg => rfg.Results);
            var testCaseIds = new List<int>();

            if (resultsToQuery.Any())
            {
                // API supports only 100 results at a time
                Parallel.ForEach(resultsToQuery.Split(100), resultList =>
                {
                    var query = new TestResultsQuery()
                    {
                        Fields = TestResultFieldsToQuery,
                        Results = resultList
                    };

                    var ids = _tcmApiHelper.GetTestResultsByQueryAsync(query)
                        .SyncResult().Results
                        .Where(r => !r.IsTestFlaky())
                        .Select(r => r.TestCaseReferenceId);

                    if (ids.Any())
                    {
                        lock (testSummary)
                        {
                            testCaseIds.AddRange(ids);
                        }
                    }
                });
            }

            return testCaseIds;
        }
    }
}