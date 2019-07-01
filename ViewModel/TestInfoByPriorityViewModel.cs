using System.Collections.Generic;
using System.Runtime.Serialization;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.ViewModel.Helpers;

namespace EmailReportFunction.ViewModel
{
    [DataContract]
    public class TestInfoByPriorityViewModel
    {
        [DataMember]
        public int Priority { get; set; }

        [DataMember]
        public string PassingRate { get; set; }

        [DataMember]
        public int TestCount { get; set; }

        public TestInfoByPriorityViewModel()
        {
        }

        public TestInfoByPriorityViewModel(int priority, Dictionary<TestOutcomeForPriority, int> testCountByOutcome,
            bool includeOthersInTotal)
        {
            Priority = priority;

            TestCount = TestResultsHelper.GetTotalTestCountBasedOnUserConfiguration(
                testCountByOutcome, includeOthersInTotal);

            if (TestCount > 0)
            {
                var passingTests = GetPassingTestCountByOutcome(testCountByOutcome);

                PassingRate = TestResultsHelper.GetTestOutcomePercentageString(passingTests, TestCount);
            }
        }

        private static int GetPassingTestCountByOutcome(Dictionary<TestOutcomeForPriority, int> testCountByOutcome)
        {
            return testCountByOutcome.ContainsKey(TestOutcomeForPriority.Passed)
                ? testCountByOutcome[TestOutcomeForPriority.Passed]
                : 0;
        }
    }
}