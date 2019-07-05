using EmailReportFunction.PostProcessor;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.DataProviders
{
    public interface IDataProviderFactory
    {
        IDataPostProcessor PostProcessor { get; }

        IDataProvider<T> GetDataProvider<T>();
    }
}
