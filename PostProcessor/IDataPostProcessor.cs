using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailReportFunction.PostProcessor
{
    /// <summary>
    /// Post Processor interface for processing collated data from providers
    /// </summary>
    public interface IDataPostProcessor
    {
        void PostProcessData();
    }
}
