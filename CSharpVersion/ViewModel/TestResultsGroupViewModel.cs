using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using EmailReportFunction.Config;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.Utils;
using EmailReportFunction.ViewModel.Helpers;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace EmailReportFunction.ViewModel
{
    [DataContract]
    public class TestResultsGroupViewModel
    {
        [DataMember]
        public List<TestResultViewModel> FailedTests { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public List<TestResultViewModel> OtherTests { get; set; }

        [DataMember]
        public List<TestResultViewModel> PassedTests { get; set; }

        public TestResultsGroupViewModel(TestResultsGroupData resultsGroupData, EmailReportConfiguration emailReportConfig)
        {
            SetGroupName(resultsGroupData, emailReportConfig);
            FailedTests = GetTestResultViewModels(resultsGroupData, emailReportConfig.PipelineConfiguration, TestOutcome.Failed);
            PassedTests = GetTestResultViewModels(resultsGroupData, emailReportConfig.PipelineConfiguration, TestOutcome.Passed);
            OtherTests = GetTestResultViewModels(resultsGroupData, emailReportConfig.PipelineConfiguration,
                EnumHelper.GetEnumsExcept(TestOutcome.Failed, TestOutcome.Passed));
        }

        private void SetGroupName(TestResultsGroupData resultsGroupData, EmailReportConfiguration emailReportConfig)
        {
            var groupTestResultsBy = emailReportConfig.ReportDataConfiguration.GroupTestResultsBy;

            GroupName = groupTestResultsBy == TestResultsGroupingType.Priority ? 
                PriorityDisplayNameHelper.GetDisplayName(resultsGroupData.GroupName) :
                resultsGroupData.GroupName;
        }

        private static List<TestResultViewModel> GetTestResultViewModels(TestResultsGroupData resultsGroupData,
            PipelineConfiguration config, params TestOutcome[] testOutcomes)
        {
            return resultsGroupData.GetTestResultsByOutcomes(testOutcomes)
                .Select(result => new TestResultViewModel(result, config)).ToList();
        }
    }
}