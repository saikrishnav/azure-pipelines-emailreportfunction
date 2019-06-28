using System;
using System.Collections.Generic;
using System.Text;

namespace EmailReportFunction.Exceptions
{
    [Serializable]
    public class InvalidTestResultDataException : EmailReportException
    {
        public InvalidTestResultDataException(string message)
            : base($"InvalidTestResultDataException: {message}")
        {
        }
    }
}
