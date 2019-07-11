using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EmailReportFunction.Config;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.Utils;
using EmailReportFunction.ViewModel.Helpers;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace EmailReportFunction.ViewModel
{
    [DataContract]
    public class TestResultViewModel
    {
        private const int StackTraceLineCount = 5;

        [DataMember]
        public List<WorkItemViewModel> AssociatedBugs { get; set; }

        [DataMember]
        public string CreateBugLink { get; set; }

        [DataMember]
        public string Duration { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public BuildReferenceViewModel FailingSinceBuild { get; set; }

        [DataMember]
        public ReleaseReferenceViewModel FailingSinceRelease { get; set; }

        [DataMember]
        public string FailingSinceTime { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Owner { get; set; }

        [DataMember]
        public string Priority { get; set; }

        [DataMember]
        public string StackTrace { get; set; }

        [DataMember]
        public string TestCaseTitle { get; set; }

        [DataMember]
        public string TestOutcome { get; set; }

        [DataMember]
        public string Url { get; set; }

        public TestResultViewModel(TestResultData testResultDto, PipelineConfiguration config)
        {
            TestCaseResult result = testResultDto.TestResult;
            Id = result.Id;
            TestCaseTitle = result.TestCaseTitle;
            ErrorMessage = StringUtils.ReplaceNewlineWithBrTag(result.ErrorMessage);
            TestOutcome = result.Outcome;
            StackTrace = StringUtils.ReplaceNewlineWithBrTag(
                StringUtils.GetFirstNLines(result.StackTrace, StackTraceLineCount));

            if (result.Priority != 255)
            {
                Priority = PriorityDisplayNameHelper.GetDisplayName(result.Priority.ToString());
            }

            InitializeAssociatedBugs(config, testResultDto.AssociatedBugs);

            Url = LinkHelper.GetTestResultLink(config, result.TestRun.Id, Id);
            Owner = result.Owner?.DisplayName;

            bool failingSinceNotCurrent;
            switch (config)
            {
                case ReleaseConfiguration releaseConfig:
                    failingSinceNotCurrent = result.FailingSince?.Release?.Id != releaseConfig.Id;
                    break;

                //TODO case Config.BuildConfiguration buildConfig:
                //    failingSinceNotCurrent = result.FailingSince?.Build?.Id != buildConfig.BuildId;
                //    break;

                default:
                    throw new NotSupportedException();
            }

            if (result.FailingSince != null && failingSinceNotCurrent)
            {
                FailingSinceTime = DateTimeHelper.GetLocalTimeWithTimeZone(result.FailingSince.Date);
                if (result.FailingSince.Release != null)
                {
                    FailingSinceRelease = new ReleaseReferenceViewModel(config, result.FailingSince.Release);
                }

                if (result.FailingSince.Build != null)
                {
                    FailingSinceBuild = new BuildReferenceViewModel(config, result.FailingSince.Build);
                }
            }

            Duration = TimeSpanFormatter.FormatDuration(TimeSpan.FromMilliseconds(result.DurationInMs),
                true);

            CreateBugLink = LinkHelper.GetCreateBugLinkForTest(config, testResultDto.TestResult);
        }

        private void InitializeAssociatedBugs(PipelineConfiguration config, IEnumerable<WorkItem> associatedBugs)
        {
            if (associatedBugs == null)
            {
                return;
            }

            AssociatedBugs = new List<WorkItemViewModel>();

            foreach (WorkItem workItem in associatedBugs)
            {
                if (workItem.Id.HasValue)
                {
                    AssociatedBugs.Add(new WorkItemViewModel(config, workItem));
                }
            }
        }
    }
}