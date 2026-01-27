using System.ComponentModel.DataAnnotations;

namespace MyDailyJournal.Models
{
    public class StreakTracking
    {
        [Key]
        public int Id { get; set; }
        
        public int CurrentStreak { get; set; } = 0; // How many days in a row?
        
        public int LongestStreak { get; set; } = 0; // Your best streak ever!
        
        public DateTime? LastEntryDate { get; set; } // When did you write last?
        
        public int TotalEntries { get; set; } = 0; // Total entries ever
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}