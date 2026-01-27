namespace MyDailyJournal.Data
{
    public static class DatabasePath
    {
        public static string GetDatabasePath()
        {
            // Platform-specific database location
            var dbPath = Path.Combine(
                FileSystem.AppDataDirectory,
                "journal.db"
            );
            
            return dbPath;
        }
    }
}   