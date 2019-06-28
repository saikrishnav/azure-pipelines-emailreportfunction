using Microsoft.EmailTask.EmailReport.ViewModel.Helpers;
using Microsoft.TeamFoundation.Tasks.Common.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.EmailTask.EmailReport.Dto;

namespace Microsoft.EmailTask.EmailReport.ViewModel
{
    [DataContract]
    public class PhaseViewModel
    {
        [DataMember]
        public DeploymentJobViewModel DeploymentJob { get; set; }

        [DataMember]
        public string TasksDuration { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Rank { get; set; }

        public PhaseViewModel()
        {

        }

        public PhaseViewModel(PhaseDto phase)
        {
            Status = phase.Status;
            Rank = phase.Rank;
            Name = phase.Name;
            InitializeDeploymentJobs(phase);
        }

        private void InitializeDeploymentJobs(PhaseDto phase)
        {
            IList<JobDto> deploymentJobs = phase.Jobs;

            if (deploymentJobs.Any())
            {
                DeploymentJob = new DeploymentJobViewModel(deploymentJobs);
                InitializeTasksDuration();
            }
            else
            {
                // This can happen if we have an empty phase or a phase with only disabled steps
                Log.LogWarning($"No deployment jobs found in phase {Name}");
            }
        }

        private void InitializeTasksDuration()
        {
            // Evaluate job duration and format it
            if (DeploymentJob.MaxTaskFinishTime.HasValue && DeploymentJob.MinTaskStartTime.HasValue)
            {
                TasksDuration = $"{TimeSpanFormatter.FormatDuration(DeploymentJob.MaxTaskFinishTime.Value.Subtract(DeploymentJob.MinTaskStartTime.Value), true)}";
            }
        }
    }
}
