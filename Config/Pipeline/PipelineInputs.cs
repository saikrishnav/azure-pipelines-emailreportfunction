using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.Pipeline
{
    public class PipelineInputs
    {
        public List<int> SourcePipelineDefinitionIds { get; set; }

        public int TargetReleaseId { get; set; }

        public int SourceEnvironmentDefinitionId { get; set; }

        public int TargetEnvironmentId { get; set; }

        public int TargetBuildId { get; set; }

        public int PhaseId { get; set; }

        public List<string> ExpectedTags { get; set; }

        public List<string> ReliabilityBugTags { get; set; }

        public List<string> TasksToSkip { get; set; }

        public string TargetWorkflowName { get; set; }

        public string TargetEnvironmentName { get; set; }

        public int BuildId { get; set; }

        public string SourceBranchFilter { get; set; }

        public string WebUrl { get; set; }

        public string Workflow { get; set; }

        public bool PullFlakyTestsFromBugs { get; set; }

        public bool SkipVSTSWorkItemFields { get; set; }
    }
}
