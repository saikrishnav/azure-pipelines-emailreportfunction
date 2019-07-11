using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace EmailReportFunction.Config
{
    [DataContract]
    public abstract class PipelineConfiguration
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string ServerUri { get; set; }

        [DataMember]
        public string ProjectId { get; set; }

        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public bool UsePreviousEnvironment { get; set; }

        public VssCredentials Credentials { get; set; }

        public abstract string TestTabLink { get; }

        public virtual PipelineConfiguration Clone()
        {
            return (PipelineConfiguration)this.MemberwiseClone();
        }
    }
}