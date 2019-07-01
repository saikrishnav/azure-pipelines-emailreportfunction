using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.Exceptions;
using EmailReportFunction.Utils;
using EmailReportFunction.ViewModel.Helpers;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace EmailReportFunction.ViewModel
{
    [DataContract]
    public class EmailReportViewModel
    {
        private Regex environmentStatusMatchRegex = new Regex("{environmentStatus}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private Regex passPercentageMatchRegex = new Regex("{passPercentage}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        [DataMember]
        public TestResultSummaryViewModel AllTests { get; set; }

        [DataMember]
        public List<ArtifactViewModel> Artifacts { get; set; }

        [DataMember]
        public List<ChangeViewModel> AssociatedChanges { get; set; }

        [DataMember]
        public bool DataMissing { get; set; }

        [DataMember]
        public string EmailSubject { get; set; }

        [DataMember]
        public bool HasFailedTests { get; set; }

        [DataMember]
        public bool HasFilteredTests { get; set; }

        [DataMember]
        public bool HasTaskFailures { get; set; }

        [DataMember]
        public bool HasCanceledPhases { get; set; }

        [DataMember]
        public int MaxTestResultsToShow { get; set; }

        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public ReleaseViewModel Release { get; set; }

        [DataMember]
        public BuildReferenceViewModel Build { get; set; }

        [DataMember]
        public List<PhaseViewModel> Phases { get; set; }

        [DataMember]
        public PhaseIssuesViewModel PhaseIssuesSummary { get; set; }

        [DataMember]
        public List<TestSummaryGroupViewModel> SummaryGroups { get; set; }

        [DataMember]
        public List<TestResultsGroupViewModel> TestResultsGroups { get; set; }

        [DataMember]
        public string TestTabLink { get; set; }

        [DataMember]
        public bool ShowAssociatedChanges { get; set; }

        public EmailReportViewModel()
        {
        }

        public EmailReportViewModel(EmailReportDto emailReportDto, EmailReportConfiguration emailReportConfiguration)
        {
            var config = emailReportConfiguration.PipelineConfiguration;
            ProjectName = config.ProjectName;
            HasTaskFailures = emailReportDto.HasFailedTasks();
            Release = emailReportDto.GetReleaseViewModel(config);
            Build = emailReportDto.GetBuildViewModel(config);
            Artifacts = emailReportDto.GetArtifactViewModels(config);

            HasCanceledPhases = emailReportDto.HasCanceledPhases();
            InitializePhases(emailReportDto);

            EmailSubject = GetMailSubject(emailReportDto, emailReportConfiguration);
            HasFailedTests = emailReportDto.HasFailedTests(emailReportConfiguration.IncludeOthersInTotalCount);

            var summaryGroupDto = emailReportDto.TestSummaryGroups?.First();

            if (emailReportDto.Summary != null)
            {
                AllTests = new TestResultSummaryViewModel(emailReportDto.Summary,
                       config,
                       emailReportConfiguration.IncludeOthersInTotalCount);
            }

            InitializeSummaryGroupViewModel(emailReportDto, emailReportConfiguration, config);
            ShowAssociatedChanges = emailReportConfiguration.IncludeAssociatedChanges;

            if (emailReportConfiguration.IncludeAssociatedChanges)
            {
                InitializeAssociatedChanges(emailReportDto, config);
            }

            InitializeTestResultGroups(emailReportDto, emailReportConfiguration, config);

            TestTabLink = config.TestTabLink;      
            DataMissing = emailReportDto.DataMissing;
        }

        #region Helpers

        private void InitializeTestResultGroups(EmailReportDto emailReportDto,
            EmailReportConfiguration emailReportConfig,
            PipelineConfiguration config)
        {
            TestResultsGroups = new List<TestResultsGroupViewModel>();

            if (emailReportDto.FilteredResults != null)
            {
                foreach (var testSummaryGroup in emailReportDto.FilteredResults)
                {
                    var testResultsGroupViewModel = new TestResultsGroupViewModel(testSummaryGroup, emailReportConfig);

                    TestResultsGroups.Add(testResultsGroupViewModel);
                }
            }

            HasFilteredTests = emailReportDto.HasFilteredTests;
        }

        private void InitializeAssociatedChanges(EmailReportDto emailReportDto, PipelineConfiguration config)
        {
            if (emailReportDto.AssociatedChanges?.Any() == true)
            {
                AssociatedChanges = new List<ChangeViewModel>();
                foreach (var associatedChange in emailReportDto.AssociatedChanges)
                {
                    AssociatedChanges.Add(new ChangeViewModel(associatedChange, config));
                }
            }
        }

        private void InitializePhases(EmailReportDto emailReportDto)
        {
            Phases = new List<PhaseViewModel>();
            if (emailReportDto.Phases?.Any() != true)
            {
                return;
            }

            foreach (var phase in emailReportDto.Phases)
            {
                Phases.Add(new PhaseViewModel(phase));
            }

            if (HasCanceledPhases)
            {
                PhaseIssuesSummary = new PhaseIssuesViewModel(emailReportDto.Phases);
            }
        }

        private string GetMailSubject(EmailReportDto emailReportDto,
            EmailReportConfiguration emailReportConfig)
        {
            var userDefinedSubject = emailReportConfig.EmailSubject;

            if (string.IsNullOrWhiteSpace(userDefinedSubject))
            {
                throw new EmailReportException("Email subject not set");
            }

            string subject;

            if (passPercentageMatchRegex.IsMatch(userDefinedSubject))
            {
                var passPercentage = GetPassPercentage(emailReportDto,
                    emailReportConfig.IncludeOthersInTotalCount);

                subject = passPercentageMatchRegex.Replace(userDefinedSubject, passPercentage);
            }
            else
            {
                subject = userDefinedSubject;
            }

            if (environmentStatusMatchRegex.IsMatch(subject))
            {
                subject = environmentStatusMatchRegex.Replace(subject, emailReportDto.GetEnvironmentStatus());
            }
            return subject;
        }

        private string GetPassPercentage(EmailReportDto emailReportDto, bool includeOthersInTotal)
        {
            var summary = emailReportDto.Summary;
            var totalTests = 0;
            var passedTests = 0;
            var failedTests = 0;

            if (summary != null)
            {
                if (summary.AggregatedResultsAnalysis.ResultsByOutcome.ContainsKey(TestOutcome.Passed))
                {
                    passedTests = summary.AggregatedResultsAnalysis.ResultsByOutcome[TestOutcome.Passed].Count;

                }
                if (summary.AggregatedResultsAnalysis.ResultsByOutcome.ContainsKey(TestOutcome.Failed))
                {
                    failedTests = summary.AggregatedResultsAnalysis.ResultsByOutcome[TestOutcome.Failed].Count;
                }

                totalTests = summary.AggregatedResultsAnalysis.TotalTests;

                if(!includeOthersInTotal)
                {
                    totalTests = passedTests + failedTests;
                }
            }

            return TestResultsHelper.GetTestOutcomePercentageString(passedTests, totalTests);
        }

        private void InitializeSummaryGroupViewModel(EmailReportDto emailReportDto,
            EmailReportConfiguration emailReportConfiguration, PipelineConfiguration config)
        {
            SummaryGroups = new List<TestSummaryGroupViewModel>();
            if (emailReportDto.TestSummaryGroups != null)
            {
                foreach (var testSummaryGroup in emailReportDto.TestSummaryGroups)
                {
                    if (emailReportConfiguration.GroupTestSummaryBy
                        .Any(group => group == testSummaryGroup.GroupingType))
                    {
                        // TODO - Log.LogVerbose($"Creating summary group viewmodel for {testSummaryGroupDto.GroupedBy}");
                        SummaryGroups.Add(new TestSummaryGroupViewModel(testSummaryGroup, config,
                            emailReportConfiguration.IncludeOthersInTotalCount));
                    }
                }
            }
        }

        #endregion
    }
}