using System.Runtime.Serialization;
using EmailReportFunction.Config;
using EmailReportFunction.Utils;
using EmailReportFunction.ViewModel.Helpers;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;

namespace EmailReportFunction.ViewModel
{
    [DataContract]
    public class ArtifactViewModel
    {
        [DataMember]
        public string ArtifactDefinitionUrl { get; set; }

        [DataMember]
        public string BranchName { get; set; }

        [DataMember]
        public string BuildSummaryUrl { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public bool IsPrimary { get; set; }

        public ArtifactViewModel(Artifact artifact, PipelineConfiguration config)
        {
            Version = GetArtifactInfo(artifact, ArtifactDefinitionConstants.Version);

            BranchName = GetArtifactInfo(artifact, ArtifactDefinitionConstants.Branch);

            Name = artifact.Alias;

            IsPrimary = artifact.IsPrimary;

            BuildSummaryUrl = LinkHelper.GetBuildSummaryLink(artifact, config);
            ArtifactDefinitionUrl = LinkHelper.GetBuildDefinitionLink(artifact, config);
        }

        private string GetArtifactInfo(Artifact artifact, string key)
        {
            if (artifact.DefinitionReference.ContainsKey(key))
            {
                return artifact.DefinitionReference[key].Name;
            }

            return null;
        }
    }
}