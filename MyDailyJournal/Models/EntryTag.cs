using System.ComponentModel.DataAnnotations;
namespace MyDailyJournal.Models
{
    public class EntryTag
    {
        [Key]
        public int Id { get; set; }
        
        public int JournalEntryId { get; set; }
        public virtual JournalEntry JournalEntry { get; set; }
        
        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}