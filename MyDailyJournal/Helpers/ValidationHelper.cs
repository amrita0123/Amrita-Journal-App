namespace MyDailyJournal.MauiBlazor.Helpers
{
    /// <summary>
    /// Helper methods for validation
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validate PIN format (4-6 digits)
        /// </summary>
        public static bool IsValidPin(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
                return false;
            
            if (pin.Length < 4 || pin.Length > 6)
                return false;
            
            return pin.All(char.IsDigit);
        }
        
        /// <summary>
        /// Validate email format
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Validate string is not empty and meets minimum length
        /// </summary>
        public static bool IsValidString(string value, int minLength = 1)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Length >= minLength;
        }
    }
}