using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.Pipeline
{
    public class ChangeData
    {
        public string Id { get; set; }

        public IdentityRef Author { get; set; }

        public Uri Location { get; set; }

        public DateTime? Timestamp { get; set; }

        public string Message { get; set; }
    }
}
