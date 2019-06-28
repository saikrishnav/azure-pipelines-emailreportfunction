using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.EmailTask.EmailReport.Config;
using Microsoft.EmailTask.EmailReport.Dto;
using Microsoft.EmailTask.EmailReport.Utils;
using Microsoft.EmailTask.EmailReport.ViewModel.Helpers;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using GroupTestResultsBy = Microsoft.EmailTask.EmailReport.Config.GroupTestResultsBy;

namespace Microsoft.EmailTask.EmailReport.ViewModel
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

        public TestResultsGroupViewModel(TestResultsGroupDto resultsGroupDto, 
            EmailReportConfiguration emailReportConfig, 
            BaseConfiguration config)
        {
            SetGroupName(resultsGroupDto, emailReportConfig);
            FailedTests = GetTestResultViewModels(resultsGroupDto, config, TestOutcome.Failed);
            PassedTests = GetTestResultViewModels(resultsGroupDto, config, TestOutcome.Passed);
            OtherTests = GetTestResultViewModels(resultsGroupDto, config,
                EnumHelper.GetEnumsExcept(TestOutcome.Failed, TestOutcome.Passed));
        }

        private void SetGroupName(TestResultsGroupDto resultsGroupDto, EmailReportConfiguration emailReportConfig)
        {
            GroupTestResultsBy groupTestResultsBy = emailReportConfig.TestResultsConfiguration.GroupTestResultsBy;

            GroupName = groupTestResultsBy == GroupTestResultsBy.Priority ? 
                PriorityDisplayNameHelper.GetDisplayName(resultsGroupDto.GroupName) : 
                resultsGroupDto.GroupName;
        }

        private static List<TestResultViewModel> GetTestResultViewModels(TestResultsGroupDto resultsGroupDto,
            BaseConfiguration config, params TestOutcome[] testOutcomes)
        {
            return resultsGroupDto.GetTestResultsByOutcomes(testOutcomes)
                .Select(result => new TestResultViewModel(result, config)).ToList();
        }
    }
}