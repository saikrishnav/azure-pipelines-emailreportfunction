using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmailReportFunction.Utils
{
    public static class ReleaseEnvironmentExtensions
    {
        public static List<ReleaseDeployPhase> GetPhases(this ReleaseEnvironment source)
        {
            List<ReleaseDeployPhase> phases = new List<ReleaseDeployPhase>();
            if (source?.DeploySteps != null && source.DeploySteps.Any())
            {
                DeploymentAttempt deploymentAttempt = source.GetMaxDeploymentAttempt();

                foreach (var releaseDeployPhase in deploymentAttempt.ReleaseDeployPhases)
                {
                    phases.Add(releaseDeployPhase);
                }
            }

            return phases;
        }

        public static List<ReleaseTask> GetReleaseTasks(this ReleaseEnvironment source)
        {
            List<ReleaseTask> tasks = new List<ReleaseTask>();

            if (source?.DeploySteps != null && source.DeploySteps.Any())
            {
                DeploymentAttempt deploymentAttempt = source.GetMaxDeploymentAttempt();

                foreach (var releaseDeployPhase in deploymentAttempt.ReleaseDeployPhases)
                {
                    foreach (var deploymentJob in releaseDeployPhase.DeploymentJobs)
                    {
                        tasks.AddRange(deploymentJob.Tasks);
                    }
                }
            }

            return tasks;
        }

        public static DeploymentAttempt GetMaxDeploymentAttempt(this ReleaseEnvironment source)
        {
            return source.DeploySteps.TakeMax(step => step.Attempt);
        }

        public static bool HasFailedTasks(this ReleaseEnvironment source)
        {
            IList<ReleaseTask> tasks = GetReleaseTasks(source);

            return !tasks.Any() ||
                tasks.Any(task => task.Status == TaskStatus.Failed);
        }

        public static ReleaseTask GetFailedTask(this ReleaseEnvironment source)
        {
            IList<ReleaseTask> tasks = GetReleaseTasks(source);
            return tasks.FirstOrDefault(task => task.Status == TaskStatus.Failed);
        }

        public static bool HasPartiallySucceededTasks(this ReleaseEnvironment source)
        {
            IList<ReleaseTask> tasks = GetReleaseTasks(source);

            return !tasks.Any() ||
                tasks.Any(task => task.Status == TaskStatus.PartiallySucceeded);
        }
    }
}
