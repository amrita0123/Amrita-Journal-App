namespace MyDailyJournal.Helpers
{
    /// <summary>
    /// Helper class for password/PIN hashing and verification
    /// Uses BCrypt for secure password hashing
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Hash a password or PIN using BCrypt
        /// </summary>
        /// <param name="password">Plain text password to hash</param>
        /// <returns>Hashed password</returns>
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        
        /// <summary>
        /// Verify a password against a hash
        /// </summary>
        /// <param name="password">Plain text password to verify</param>
        /// <param name="hash">Hash to verify against</param>
        /// <returns>True if password matches, false otherwise</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }
    }
}