using EmailReportFunction.Config;
using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.Config.TestResults;
using EmailReportFunction.Exceptions;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailReportFunction.Utils
{
    public static class EmailReportDtoExtensions
    {
        public static bool HasFailedTests(this EmailReportDto source, bool includeOthers)
        {
            if (source.Summary == null)
            {
                return false;
            }

            if (!includeOthers)
            {
                return source.Summary.AggregatedResultsAnalysis.ResultsByOutcome.ContainsKey(TestOutcome.Failed) &&
                    source.Summary.AggregatedResultsAnalysis.ResultsByOutcome[TestOutcome.Failed].Count > 0;
            }

            if (source.HasPassedTests())
            {
                return source.Summary.AggregatedResultsAnalysis.TotalTests -
                    source.Summary.AggregatedResultsAnalysis.ResultsByOutcome[TestOutcome.Passed].Count > 0;
            }

            return source.Summary.AggregatedResultsAnalysis.TotalTests > 0;
        }

        public static bool HasCanceledPhases(this EmailReportDto source)
        {
            if (source.Phases == null)
            {
                return false;
            }

            return source.Phases.Any(p => p.Jobs != null && p.Jobs.Any(j => j.JobStatus == TaskStatus.Canceled));
        }

        public static bool HasPassedTests(this EmailReportDto source)
        {
            if (source.Summary == null)
            {
                return false;
            }

            return source.Summary.AggregatedResultsAnalysis.ResultsByOutcome.ContainsKey(TestOutcome.Passed) &&
                    source.Summary.AggregatedResultsAnalysis.ResultsByOutcome[TestOutcome.Passed].Count > 0;
        }

        public static void Merge(this EmailReportDto source, EmailReportDto target)
        {
            switch (source)
            {
                case ReleaseEmailReportDto releaseSource:
                    var releaseTarget = target as ReleaseEmailReportDto
                        ?? throw new NotSupportedException();

                    if (releaseTarget.Artifacts != null)
                    {
                        ThrowIfNotNull(releaseSource.Artifacts, nameof(releaseSource.Artifacts));
                        releaseSource.Artifacts = releaseTarget.Artifacts;
                    }

                    if (releaseTarget.Release != null)
                    {
                        ThrowIfNotNull(releaseSource.Release, nameof(releaseTarget.Release));
                        releaseSource.Release = releaseTarget.Release;
                    }

                    if (releaseTarget.Environment != null)
                    {
                        ThrowIfNotNull(releaseSource.Environment, nameof(releaseTarget.Environment));
                        releaseSource.Environment = releaseTarget.Environment;
                    }

                    if (releaseTarget.LastCompletedRelease != null)
                    {
                        ThrowIfNotNull(releaseSource.LastCompletedRelease, nameof(releaseSource.LastCompletedRelease));
                        releaseSource.LastCompletedRelease = releaseTarget.LastCompletedRelease;
                    }

                    if (releaseTarget.LastCompletedEnvironment != null)
                    {
                        ThrowIfNotNull(releaseSource.LastCompletedEnvironment, nameof(releaseSource.LastCompletedEnvironment));
                        releaseSource.LastCompletedEnvironment = releaseTarget.LastCompletedEnvironment;
                    }

                    break;

                //TODO - case BuildEmailReportDto buildSource:
                //    var buildTarget = target as BuildEmailReportDto
                //        ?? throw new NotSupportedException();

                //    if (buildTarget.Build != null)
                //    {
                //        ThrowIfNotNull(buildSource.Build, nameof(buildSource.Build));
                //        buildSource.Build = buildTarget.Build;
                //    }

                //    if (buildTarget.Timeline != null)
                //    {
                //        ThrowIfNotNull(buildSource.Timeline, nameof(buildSource.Timeline));
                //        buildSource.Timeline = buildTarget.Timeline;
                //    }

                //    if (buildTarget.LastCompletedBuild != null)
                //    {
                //        ThrowIfNotNull(buildSource.LastCompletedBuild, nameof(buildSource.LastCompletedBuild));
                //        buildSource.LastCompletedBuild = buildTarget.LastCompletedBuild;
                //    }

                //    if (buildTarget.LastCompletedTimeline != null)
                //    {
                //        ThrowIfNotNull(buildSource.LastCompletedTimeline, nameof(buildSource.LastCompletedTimeline));
                //        buildSource.LastCompletedTimeline = buildTarget.LastCompletedTimeline;
                //    }

                //    break;

                default:
                    throw new NotSupportedException();
            }

            if (target.TestSummaryGroups != null)
            {
                ThrowIfNotNull(source.TestSummaryGroups, nameof(source.TestSummaryGroups));

                source.TestSummaryGroups = target.TestSummaryGroups;
            }

            if (target.AssociatedChanges != null)
            {
                ThrowIfNotNull(source.AssociatedChanges, nameof(source.AssociatedChanges));

                source.AssociatedChanges = target.AssociatedChanges;
            }

            if (target.CreatedBy != null)
            {
                ThrowIfNotNull(source.CreatedBy, nameof(source.CreatedBy));

                source.CreatedBy = target.CreatedBy;
            }

            if (target.FailedTestOwners != null)
            {
                ThrowIfNotNull(source.FailedTestOwners, nameof(source.FailedTestOwners));

                source.FailedTestOwners = target.FailedTestOwners;
            }

            if (target.FilteredResults != null)
            {
                ThrowIfNotNull(source.FilteredResults, nameof(source.FilteredResults));

                source.FilteredResults = target.FilteredResults;
            }

            if (target.HasFilteredTests)
            {
                ThrowIfTrue(source.HasFilteredTests, nameof(source.HasFilteredTests));

                source.HasFilteredTests = target.HasFilteredTests;
            }

            if (target.Summary != null)
            {
                ThrowIfNotNull(source.Summary, nameof(source.Summary));

                source.Summary = target.Summary;
            }

            if (target.Phases != null)
            {
                ThrowIfNotNull(source.Phases, nameof(source.Phases));

                source.Phases = target.Phases;

            }
        }

        private static TestSummaryGroup GetSummaryGroup(this EmailReportDto source)
        {
            var summaryGroup = source.TestSummaryGroups?.First();
            if (summaryGroup == null)
            {
                // TODO - Log.LogError("summary group not found");
            }
            return summaryGroup;
        }

        private static bool HasTestsWithOutcomes(TestSummaryGroup summaryGroup,
            params TestOutcome[] failureOutcomes)
        {
            if (summaryGroup == null)
            {
                return false;
            }

            return failureOutcomes.Any(failureOutcome =>
                summaryGroup.Runs.Any(testSummaryItemDto =>
                    testSummaryItemDto.TestCountByOutCome.ContainsKey(failureOutcome) &&
                    testSummaryItemDto.TestCountByOutCome[failureOutcome] > 0));
        }

        private static void ThrowIfNotNull(object field, string fieldName)
        {
            if (field != null)
            {
                throw new MultipleDataForDtoException(fieldName);
            }
        }

        private static void ThrowIfTrue(bool field, string fieldName)
        {
            if (field)
            {
                throw new MultipleDataForDtoException(fieldName);
            }
        }
    }
}
