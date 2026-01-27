using System.ComponentModel.DataAnnotations;

namespace MyDailyJournal.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } // "Birthday", "Sports", etc.
        
        public bool IsCustom { get; set; } = false; // Did the kid create this tag?
        
        public int UsageCount { get; set; } = 0; // How many times used
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Connects to journal entries using this tag
        public virtual ICollection<EntryTag> EntryTags { get; set; }
    }
}