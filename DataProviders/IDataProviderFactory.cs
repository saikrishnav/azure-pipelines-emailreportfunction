using EmailReportFunction.Config.Pipeline;
using EmailReportFunction.PostProcessor;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.DataProviders
{
    public interface IDataProviderFactory
    {
        IEnumerable<IDataProvider> GetPipelineDataProviders();

        IEnumerable<IDataProvider> GetPostProcessors();
    }
}
