using System.Collections.Generic;
using System.Runtime.Serialization;
using EmailReportFunction.Config;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.Utils;

namespace EmailReportFunction.ViewModel
{
    [DataContract]
    public class TestSummaryGroupViewModel
    {
        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public List<TestSummaryItemViewModel> SummaryItems { get; set; }

        [DataMember]
        private SortedSet<int> SupportedPriorityColumns { get; set; }

        public TestSummaryGroupViewModel(TestSummaryGroup testSummaryGroup, PipelineConfiguration config,
            bool includeOthersInTotal)
        {
            GroupName = testSummaryGroup.GroupingType.GetDescription();

            InitializeSummaryItems(testSummaryGroup, config, includeOthersInTotal);

            InitializeSupportedPriorityColumns();
        }

        #region Helpers

        private void InitializeSummaryItems(TestSummaryGroup testSummaryGroup, PipelineConfiguration config,
            bool includeOthersInTotal)
        {
            SummaryItems = new List<TestSummaryItemViewModel>();
            foreach (var testSummaryItem in testSummaryGroup.Runs)
            {
                SummaryItems.Add(new TestSummaryItemViewModel(testSummaryGroup.GroupingType, testSummaryItem, config, includeOthersInTotal));
            }
        }

        private void InitializeSupportedPriorityColumns()
        {
            SupportedPriorityColumns = new SortedSet<int>();

            SummaryItems.ForEach(item =>
                item.TestsByPriority.ForEach(testsByPriorityVm =>
                {
                    if (testsByPriorityVm.Priority <= EmailReportConstants.MaxSupportedPriority)
                    {
                        SupportedPriorityColumns.Add(testsByPriorityVm.Priority);
                    }
                })
                );
        }

        #endregion
    }
}