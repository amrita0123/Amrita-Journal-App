namespace MyDailyJournal.Helpers
{
    /// <summary>
    /// Helper methods for date operations
    /// </summary>
    public static class DateHelper
    {
        /// <summary>
        /// Get start of day (midnight)
        /// </summary>
        public static DateTime StartOfDay(DateTime date)
        {
            return date.Date;
        }
        
        /// <summary>
        /// Get end of day (23:59:59)
        /// </summary>
        public static DateTime EndOfDay(DateTime date)
        {
            return date.Date.AddDays(1).AddTicks(-1);
        }
        
        /// <summary>
        /// Check if two dates are the same day
        /// </summary>
        public static bool IsSameDay(DateTime date1, DateTime date2)
        {
            return date1.Date == date2.Date;
        }
        
        /// <summary>
        /// Get days between two dates
        /// </summary>
        public static int DaysBetween(DateTime date1, DateTime date2)
        {
            return (date2.Date - date1.Date).Days;
        }
        
        /// <summary>
        /// Format date for display
        /// </summary>
        public static string FormatDate(DateTime date, string format = "MMM dd, yyyy")
        {
            return date.ToString(format);
        }
        
        /// <summary>
        /// Get friendly date string (Today, Yesterday, etc.)
        /// </summary>
        public static string GetFriendlyDate(DateTime date)
        {
            var today = DateTime.Now.Date;
            var diff = (today - date.Date).Days;
            
            return diff switch
            {
                0 => "Today",
                1 => "Yesterday",
                -1 => "Tomorrow",
                _ when diff > 1 && diff < 7 => $"{diff} days ago",
                _ when diff < -1 && diff > -7 => $"In {Math.Abs(diff)} days",
                _ => date.ToString("MMM dd, yyyy")
            };
        }
    }
}