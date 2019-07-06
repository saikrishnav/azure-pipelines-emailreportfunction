using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.ViewModel;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config
{
    public abstract class AbstractReport
    {
        public bool DataMissing { get; set; }

        public List<IdentityRef> FailedTestOwners { get; set; }

        public IdentityRef CreatedBy { get; set; }

        public List<TestResultsGroupData> FilteredResults { get; set; }

        public bool HasFilteredTests { get; set; }

        public List<TestSummaryGroup> TestSummaryGroups { get; set; }

        public List<PhaseData> Phases { get; set; }

        public TestResultSummary Summary { get; set; }

        public List<ChangeData> AssociatedChanges { get; set; }

        public bool SendMailConditionSatisfied { get; set; }

        public SmtpConfiguration SmtpConfiguration { get; set; }

        public abstract bool? HasPrevGotSameFailures();

        public abstract bool HasFailedTasks();

        public abstract bool HasPrevFailedTasks();

        public abstract bool ArePrevFailedTasksSame();

        public abstract PipelineConfiguration GetPrevConfig(PipelineConfiguration config);

        public abstract string GetEnvironmentStatus();

        public abstract ReleaseViewModel GetReleaseViewModel(PipelineConfiguration config);

        public abstract BuildReferenceViewModel GetBuildViewModel(PipelineConfiguration config);

        public abstract List<ArtifactViewModel> GetArtifactViewModels(PipelineConfiguration config);

        public abstract AbstractReport CreateEmptyReportData();
    }
}
