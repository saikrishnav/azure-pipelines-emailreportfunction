using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.EmailTask.EmailReport.Config;
using Microsoft.EmailTask.EmailReport.Constants;
using Microsoft.EmailTask.EmailReport.Dto;
using Microsoft.EmailTask.EmailReport.ViewModel.Helpers;

namespace Microsoft.EmailTask.EmailReport.ViewModel
{
    [DataContract]
    public class TestSummaryItemViewModel : TestResultSummaryViewModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<TestInfoByPriorityViewModel> TestsByPriority { get; set; }

        public TestSummaryItemViewModel(GroupTestResultsBy groupedBy, TestSummaryItemDto summaryItemDto, BaseConfiguration config, bool includeOthersInTotal) :
            base( summaryItemDto, config, includeOthersInTotal)
        {
            Name = (groupedBy == GroupTestResultsBy.Priority) ? 
                PriorityDisplayNameHelper.GetDisplayName(summaryItemDto.Name) : 
                summaryItemDto.Name;

            SetupPriorityData(summaryItemDto, includeOthersInTotal);
        }

        #region Helpers

        private void SetupPriorityData(TestSummaryItemDto summaryItemDto, bool includeOthersInTotal)
        {
            TestsByPriority = new List<TestInfoByPriorityViewModel>();

            Dictionary<int, Dictionary<TestOutcomeForPriority, int>> testCountForOutcomeByPriority =
                summaryItemDto.TestCountForOutcomeByPriority;

            foreach (var priority in testCountForOutcomeByPriority.Keys)
            {
                if (priority <= EmailReportConstants.MaxSupportedPriority)
                {
                    TestsByPriority.Add(new TestInfoByPriorityViewModel(priority,
                        testCountForOutcomeByPriority[priority], includeOthersInTotal));
                }
            }
        }

        #endregion
    }
}