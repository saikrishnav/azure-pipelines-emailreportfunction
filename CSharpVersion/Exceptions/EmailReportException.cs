using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Exceptions
{
    [Serializable]
    public class EmailReportException : Exception
    {
        public EmailReportException(string message) : base(message)
        {
        }
    }
}
