using System;
using System.Text;

namespace EmailReportFunction.ViewModel.Helpers
{
    public static class TimeSpanFormatter
    {
        public static string FormatDuration(TimeSpan ts, bool includeMilliSec = false)
        {
            var sbDuration = new StringBuilder();
            if (ts.TotalHours >= 1)
            {
                sbDuration.AppendFormat("{0}:{1:00}:", (int) ts.TotalHours, ts.Minutes);
            }
            else
            {
                sbDuration.AppendFormat("{0}:", ts.Minutes);
            }
            sbDuration.AppendFormat("{0:00}", ts.Seconds);
            if (includeMilliSec)
            {
                sbDuration.AppendFormat(".{0:000}", ts.Milliseconds);
            }
            return sbDuration.ToString();
        }

        public static string FormatDurationWithUnit(TimeSpan ts)
        {
            var sbDuration = new StringBuilder();

            if ((int)ts.TotalHours >= 1)
            {
                sbDuration.AppendFormat($"{(int)ts.TotalHours}h ");
            }

            if((int)ts.Minutes >= 1)
            {
                sbDuration.AppendFormat($"{ts.Minutes}m ");
            }
            if ((int)ts.Seconds >= 1)
            {
                sbDuration.AppendFormat($"{ts.Seconds}s ");
            }

            if ((int)ts.TotalMinutes < 1)
            {
                sbDuration.AppendFormat($"{ts.Milliseconds}ms ");
            }

            return sbDuration.ToString();
        }
    }
}