using EmailReportFunction.ViewModel.Helpers;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config
{
    public class ReleaseConfiguration : PipelineConfiguration
    {
        public string RMServerUri { get; set; }

        public int ReleaseId { get; set; }

        public int EnvironmentId { get; set; }

        public int DefinitionEnvironmentId { get; set; }

        public ReleaseEnvironment Environment { get; set; }

        public Release LastCompletedRelease { get; set; }

        public ReleaseEnvironment LastCompletedEnvironment { get; set; }

        private string _testTabLink; 

        public override string TestTabLink
        {
            get
            {
                if (_testTabLink == null)
                {
                    _testTabLink = LinkHelper.GetTestTabLinkInRelease(this);
                }
                return _testTabLink;
            }
        }
    }
}
