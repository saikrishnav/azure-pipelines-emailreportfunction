using EmailReportFunction.Config;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.DataProviders
{
    public class FailedTestOwnersDataProvider : IDataProvider
    {
        private readonly ITcmApiHelper _tcmApiHelper;

        private ILogger _logger;

        public FailedTestOwnersDataProvider(ITcmApiHelper tcmApiHelper, ILogger logger)
        {
            _tcmApiHelper = tcmApiHelper;
            _logger = logger;
        }

        public async Task AddReportDataAsync(AbstractReport reportData)
        {
            var failedTestResultIds = await _tcmApiHelper.GetTestSummaryAsync(TestResultsConstants.TestRun, TestOutcome.Failed);
            List<TestCaseResult> resultsToFetch = failedTestResultIds.ResultsForGroup.SelectMany(group => group.Results).ToList();

            reportData.FailedTestOwners = await _tcmApiHelper.GetTestResultOwnersAsync(resultsToFetch);

            _logger.LogInformation("Fetched test owners data");
        }
    }
}
