using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using System.Linq;

namespace Microsoft.EmailTask.EmailReport.ViewModel
{
    [DataContract]
    public class ReleaseEnvironmentViewModel
    {
        [DataMember]
        public string EnvironmentName { get; set; }

        [DataMember]
        public string EnvironmentOwnerEmail { get; set; }

        public ReleaseEnvironmentViewModel()
        {
        }

        public ReleaseEnvironmentViewModel(ReleaseEnvironment environment)
        {
            EnvironmentName = environment?.Name;
            EnvironmentOwnerEmail = environment?.Owner?.UniqueName;
        }
    }
}