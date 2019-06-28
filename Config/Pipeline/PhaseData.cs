using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Config.Pipeline
{
    public class PhaseData
    {
        public string Name { get; set; }

        public List<JobData> Jobs { get; set; }

        public string Status { get; set; }

        public int Rank { get; set; }
    }
}
