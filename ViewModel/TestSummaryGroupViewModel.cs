using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.EmailTask.EmailReport.Config;
using Microsoft.EmailTask.EmailReport.Constants;
using Microsoft.EmailTask.EmailReport.Dto;
using Microsoft.EmailTask.EmailReport.Utils;

namespace Microsoft.EmailTask.EmailReport.ViewModel
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

        public TestSummaryGroupViewModel(TestSummaryGroupDto testSummaryGroupDto, BaseConfiguration config,
            bool includeOthersInTotal)
        {
            GroupName = testSummaryGroupDto.GroupedBy.GetDescription();

            InitializeSummaryItems(testSummaryGroupDto, config, includeOthersInTotal);

            InitializeSupportedPriorityColumns();
        }

        #region Helpers

        private void InitializeSummaryItems(TestSummaryGroupDto testSummaryGroupDto, BaseConfiguration config,
            bool includeOthersInTotal)
        {
            SummaryItems = new List<TestSummaryItemViewModel>();
            foreach (TestSummaryItemDto testSummaryItemDto in testSummaryGroupDto.Runs)
            {
                SummaryItems.Add(new TestSummaryItemViewModel(testSummaryGroupDto.GroupedBy, testSummaryItemDto, config, includeOthersInTotal));
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