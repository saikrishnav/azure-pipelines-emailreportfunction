using System.Runtime.Serialization;
using Microsoft.EmailTask.EmailReport.Config;
using Microsoft.EmailTask.EmailReport.Utils;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace Microsoft.EmailTask.EmailReport.ViewModel
{
    [DataContract]
    public class BuildReferenceViewModel
    {
        [DataMember]
        public int Id;

        [DataMember]
        public string Number;

        [DataMember]
        public string Branch;

        [DataMember]
        public string Url;

        [DataMember]
        public string DefinitionUrl;

        [DataMember]
        public string DefinitionName;

        private BuildReferenceViewModel(int id, string number, string url, string branch = null, string definitionUrl = null, string definitionName = null)
        {
            Id = id;
            Number = number;
            Url = url;

            Branch = branch;
            DefinitionUrl = definitionUrl;
            DefinitionName = definitionName;
        }

        public BuildReferenceViewModel(BaseConfiguration config, BuildReference buildReference)
            : this(buildReference.Id,
                  buildReference.Number,
                  LinkHelper.GetBuildSummaryLink(buildReference.Id.ToString(), config))
        {
        }

        public BuildReferenceViewModel(BaseConfiguration config, TeamFoundation.Build.WebApi.Build build)
            : this(build.Id, 
                  build.BuildNumber, 
                  LinkHelper.GetBuildSummaryLink(build.Id.ToString(), config), 
                  build.SourceBranch, 
                  LinkHelper.GetBuildDefinitionLink(build.Definition.Id.ToString(), config), 
                  build.Definition.Name)
        {
        }
    }
}