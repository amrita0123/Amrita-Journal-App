using System.ComponentModel.DataAnnotations;
namespace MyDailyJournal.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } // "School", "Family", etc.
        
        public string ColorCode { get; set; } // Color for this category
        
        public string IconName { get; set; } // Icon name
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // All journal entries in this category
        public virtual ICollection<JournalEntry> JournalEntries { get; set; }
    }
}