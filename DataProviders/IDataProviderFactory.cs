using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.DataProviders
{
    public interface IDataProviderFactory
    {
        IDataProvider<T> GetDataProvider<T>();
    }
}
