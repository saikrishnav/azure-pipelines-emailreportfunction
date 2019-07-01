using System;

namespace EmailReportFunction.ViewModel.Helpers
{
    public static class DateTimeHelper
    {
        public static string GetLocalTimeWithTimeZone(DateTime? dateTime)
        {
            if (dateTime != null)
            {
                var formattedTimeStampStr = dateTime.Value.ToUniversalTime().ToString("g");
                
                return $"{formattedTimeStampStr} {TimeZoneInfo.Utc.DisplayName}";
            }

            return string.Empty;
        }
    }
}
