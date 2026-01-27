namespace MyDailyJournal.Helpers
{
    /// <summary>
    /// Helper for color operations
    /// </summary>
    public static class ColorHelper
    {
        /// <summary>
        /// Convert hex color to RGB values
        /// </summary>
        public static (int R, int G, int B) HexToRgb(string hex)
        {
            hex = hex.Replace("#", "");
            
            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);
            
            return (r, g, b);
        }
        
        /// <summary>
        /// Get color for mood type
        /// </summary>
        public static string GetMoodColor(string moodType)
        {
            return moodType.ToLower() switch
            {
                "positive" => "#4CAF50",
                "neutral" => "#FFC107",
                "negative" => "#F44336",
                _ => "#9E9E9E"
            };
        }
    }
}