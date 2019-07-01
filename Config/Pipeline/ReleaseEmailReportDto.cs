using EmailReportFunction.Utils;
using EmailReportFunction.ViewModel;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailReportFunction.Config.Pipeline
{
    public class ReleaseEmailReportDto : EmailReportDto
    {
        public List<Artifact> Artifacts { get; set; }

        public Release Release { get; set; }

        public ReleaseEnvironment Environment { get; set; }

        public Release LastCompletedRelease { get; set; }

        public ReleaseEnvironment LastCompletedEnvironment { get; set; }

        public override bool? HasPrevGotSameFailures()
        {
            // TODO - Log.LogInfo($"Using Last Completed Release: '{LastCompletedRelease?.Id}'.");
            if (LastCompletedRelease == null || LastCompletedEnvironment == null)
            {
                return false;
            }

            if (LastCompletedRelease.Id > Release.Id)
            {
                // We are in a situation where current build completed latter compared to the newer one
                // Newer one would have already evaluated the failures and sent a mail to committers anyway
                // No need to send mail again because there won't be any committers in this mail as associated changes are already evaluated by newer
                // Treat as same failures because it would be noise to M2s and other standard owners in the To-Line
                return true;
            }

            return null;
        }

        public override bool HasPrevFailedTasks()
        {
            return LastCompletedEnvironment.HasFailedTasks();
        }

        public override bool HasFailedTasks()
        {
            return Environment.HasFailedTasks();
        }

        public override PipelineConfiguration GetPrevConfig(PipelineConfiguration config)
        {
            var releaseConfig = config.Clone() as ReleaseConfiguration;
            if (releaseConfig == null)
            {
                throw new NotSupportedException();
            }

            releaseConfig.ReleaseId = LastCompletedRelease.Id;
            releaseConfig.DefinitionEnvironmentId = LastCompletedEnvironment.DefinitionEnvironmentId;
            releaseConfig.EnvironmentId = LastCompletedEnvironment.Id;
            return releaseConfig;
        }

        public override bool ArePrevFailedTasksSame()
        {
            var prevfailedTask = LastCompletedEnvironment.GetFailedTask();
            var currentFailedTask = Environment.GetFailedTask();

            // if both releases failed without executing any tasks, then they can be null 
            // otherwise, use name matching
            return (prevfailedTask == null && currentFailedTask == null)
                   || string.Equals(prevfailedTask?.Name, currentFailedTask?.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override string GetEnvironmentStatus()
        {
            if (Environment.HasFailedTasks() || this.HasCanceledPhases())
            {
                return "Failed";
            }
            else if (Environment.HasPartiallySucceededTasks())
            {
                return "Partially Succeeded";
            }
            else
            {
                return "Succeeded";
            }
        }

        public override ReleaseViewModel GetReleaseViewModel(PipelineConfiguration config)
        {
            var releaseConfig = config.Clone() as ReleaseConfiguration;
            if (releaseConfig == null)
            {
                throw new NotSupportedException();
            }

            return new ReleaseViewModel(Environment, releaseConfig);
        }

        public override BuildReferenceViewModel GetBuildViewModel(PipelineConfiguration config)
        {
            return null;
        }

        public override List<ArtifactViewModel> GetArtifactViewModels(PipelineConfiguration config)
        {
            var artifacts = new List<ArtifactViewModel>();
            if (Artifacts != null && Artifacts.Any())
            {
                foreach (Artifact artifact in Artifacts)
                {
                    artifacts.Add(new ArtifactViewModel(artifact, config));
                }
            }

            return artifacts;
        }
    }
}
