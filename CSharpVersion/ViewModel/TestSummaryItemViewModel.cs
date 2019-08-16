using System.Collections.Generic;
using System.Runtime.Serialization;
using EmailReportFunction.Config;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.ViewModel.Helpers;

namespace EmailReportFunction.ViewModel
{
    [DataContract]
    public class TestSummaryItemViewModel : TestResultSummaryViewModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<TestInfoByPriorityViewModel> TestsByPriority { get; set; }

        public TestSummaryItemViewModel(TestResultsGroupingType groupedBy, TestSummaryItem summaryItem, PipelineConfiguration config, bool includeOthersInTotal) :
            base(summaryItem, config, includeOthersInTotal)
        {
            Name = (groupedBy == TestResultsGroupingType.Priority) ? 
                PriorityDisplayNameHelper.GetDisplayName(summaryItem.Name) :
                summaryItem.Name;

            SetupPriorityData(summaryItem, includeOthersInTotal);
        }

        #region Helpers

        private void SetupPriorityData(TestSummaryItem summaryItem, bool includeOthersInTotal)
        {
            TestsByPriority = new List<TestInfoByPriorityViewModel>();

            Dictionary<int, Dictionary<TestOutcomeForPriority, int>> testCountForOutcomeByPriority =
                summaryItem.TestCountForOutcomeByPriority;

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