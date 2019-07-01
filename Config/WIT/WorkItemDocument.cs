using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.WIT
{
    public class WorkItemDocument
    {
        public JsonPatchDocument Properties { get; set; }

        public string Type { get; set; }
    }
}
