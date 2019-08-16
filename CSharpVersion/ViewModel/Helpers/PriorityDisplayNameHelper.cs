namespace EmailReportFunction.ViewModel.Helpers
{
    public static class PriorityDisplayNameHelper
    {
        public static string GetDisplayName(string priority)
        {
            int priorityInt;
            if (int.TryParse(priority, out priorityInt)
                && priorityInt == 255)
            {
                return "Priority unspecified";
            }

            return $"Priority: {priority}";
        }

    }
}
