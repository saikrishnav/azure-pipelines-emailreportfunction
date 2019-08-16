using System.Runtime.Serialization;
using EmailReportFunction.Config;
using EmailReportFunction.ViewModel.Helpers;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace EmailReportFunction.ViewModel
{
    [DataContract]
    public class ReleaseReferenceViewModel
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Url { get; set; }

        public ReleaseReferenceViewModel(PipelineConfiguration config, ReleaseReference releaseReference)
        {
            Id = releaseReference.Id;
            Name = releaseReference.Name;
            Url = LinkHelper.GetReleaseSummaryLink(config);
        }
    }
}