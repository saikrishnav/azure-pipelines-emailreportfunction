﻿using EmailReportFunction.Config.Pipeline;

namespace EmailReportFunction.Config
{
    public class EmailReportConfiguration
    {
        public PipelineType PipelineType { get; set; }
         
        public PipelineConfiguration PipelineConfiguration { get; set; }

        public MailConfiguration[] MailConfigurations { get; set; }

        public ReportDataConfiguration ReportDataConfiguration { get; set; }
    }
}
