using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Exceptions
{
    [Serializable]
    public class ReleaseDataException : EmailReportException
    {
        public ReleaseDataException(string message)
            : base($"ReleaseDataException: {message}")
        {
        }
    }
}
