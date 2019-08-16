using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Utils
{
    public static class ArtifactHelper
    {
        public static ArtifactSourceReference GetArtifactInfo(this Artifact artifact, string key)
        {
            if (artifact.DefinitionReference.ContainsKey(key))
            {
                return artifact.DefinitionReference[key];
            }

            return null;
        }
    }
}
