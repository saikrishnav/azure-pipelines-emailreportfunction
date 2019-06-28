using System.Runtime.Serialization;
using Microsoft.EmailTask.EmailReport.Config;
using Microsoft.EmailTask.EmailReport.Utils;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace Microsoft.EmailTask.EmailReport.ViewModel
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

        public ReleaseReferenceViewModel(BaseConfiguration config, ReleaseReference releaseReference)
        {
            Id = releaseReference.Id;
            Name = releaseReference.Name;
            Url = LinkHelper.GetReleaseSummaryLink(releaseReference.Id, config);
        }
    }
}