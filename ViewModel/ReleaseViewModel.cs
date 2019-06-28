using System.Runtime.Serialization;
using Microsoft.EmailTask.EmailReport.Config;
using Microsoft.EmailTask.EmailReport.Utils;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;

namespace Microsoft.EmailTask.EmailReport.ViewModel
{
    [DataContract]
    public class ReleaseViewModel
    {
        [DataMember]
        public ReleaseEnvironmentViewModel CurrentEnvironment { get; set; }

        [DataMember]
        public string ReleaseDefinitionName { get; set; }

        [DataMember]
        public string ReleaseDefinitionUrl { get; set; }

        [DataMember]
        public int ReleaseId { get; set; }

        [DataMember]
        public string ReleaseName { get; set; }

        [DataMember]
        public string ReleaseSummaryUrl { get; set; }

        [DataMember]
        public string ReleaseLogsLink { get; set; }

        public ReleaseViewModel()
        {
        }

        public ReleaseViewModel(ReleaseEnvironment currentEnvironment, ReleaseConfiguration releaseConfig)
        {
            if (currentEnvironment != null)
            {
                CurrentEnvironment = new ReleaseEnvironmentViewModel(currentEnvironment);
                ReleaseDefinitionName = currentEnvironment.ReleaseDefinitionReference
                    ?.Name;

                if (currentEnvironment.ReleaseDefinitionReference != null)
                {
                    ReleaseDefinitionUrl = LinkHelper.GetReleaseDefinitionLink(releaseConfig,
                        currentEnvironment.ReleaseDefinitionReference.Id);
                }

                ReleaseName = currentEnvironment.ReleaseReference?.Name;
            }

            ReleaseId = releaseConfig.ReleaseId;

            ReleaseSummaryUrl = LinkHelper.GetReleaseSummaryLink(releaseConfig.ReleaseId, releaseConfig);

            ReleaseLogsLink = LinkHelper.GetReleaseLogsTabLink(releaseConfig);
        }
    }
}