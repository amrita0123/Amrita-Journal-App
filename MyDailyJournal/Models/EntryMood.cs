using System.ComponentModel.DataAnnotations;
namespace MyDailyJournal.Models
{
    public class EntryMood
    {
        [Key]
        public int Id { get; set; }
        
        public int JournalEntryId { get; set; }
        public virtual JournalEntry JournalEntry { get; set; }
        
        public int MoodId { get; set; }
        public virtual Mood Mood { get; set; }
        
        public bool IsPrimary { get; set; } = false; // Is this the main mood?
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}