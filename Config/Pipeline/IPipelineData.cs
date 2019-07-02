using EmailReportFunction.Config.TestResults;
using EmailReportFunction.DataProviders;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.Config.Pipeline
{
    public interface IPipelineData
    {
        IdentityRef CreatedBy { get; }

        Task<List<ChangeData>> GetAssociatedChangesAsync();

        Task<List<IdentityRef>> GetFailedTestOwnersAsync();

        Task<IEnumerable<TestResultsGroupData>> GetFilteredTestsAsync();

        Task<TestSummaryData> GetTestSummaryDataAsync();
    }
}
