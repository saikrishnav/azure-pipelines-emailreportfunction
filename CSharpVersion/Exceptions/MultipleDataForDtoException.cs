using EmailReportFunction.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Exceptions
{
    [Serializable]
    public class MultipleDataForDtoException : EmailReportException
    {
        public MultipleDataForDtoException(string fieldInDto) :
            base($"More than one provider is providing data for the field - {fieldInDto} in {nameof(AbstractReport)}")
        {

        }
    }
}
