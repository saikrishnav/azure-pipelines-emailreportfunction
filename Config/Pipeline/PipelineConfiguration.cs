using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config
{
    public abstract class PipelineConfiguration
    {
        public VssCredentials Credentials { get; set; }

        public string ServerUri { get; set; }

        public string ProjectId { get; set; }

        public string ProjectName { get; set; }

        public bool UsePreviousEnvironment { get; set; }

        public abstract string TestTabLink { get; }

        public virtual PipelineConfiguration Clone()
        {
            return (PipelineConfiguration)this.MemberwiseClone();
        }
    }
}