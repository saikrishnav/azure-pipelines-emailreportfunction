using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EmailReportFunction.Utils
{
    public class PerformanceMeasurementBlock : IDisposable
    {
        internal const string PrefixStr = "[PERF]";

        private readonly string _name;

        private readonly ILogger _log;

        internal Stopwatch Stopwatch { get; } = new Stopwatch();

        public PerformanceMeasurementBlock(string name, ILogger log)
        {
            _name = name;
            _log = log;

            log.LogInformation($"{PrefixStr} Starting performance tracking for << {_name} >>");
            Stopwatch.Start();
        }

        public void Dispose()
        {
            Stopwatch.Stop();

            _log.LogInformation($"{PrefixStr} Time taken for << {_name} >> - {Stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
