
using System.ComponentModel.DataAnnotations;

namespace MyDailyJournal.Models
{
    public class Mood
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } // "Happy", "Sad", etc.
        
        [Required]
        public string Type { get; set; } // "Positive", "Neutral", "Negative"
        
        [Required]
        public string Emoji { get; set; } // "😊", "😢", etc.
        
        public string ColorCode { get; set; } // "#FFD700" for gold, etc.
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // This connects to journal entries that use this mood
        public virtual ICollection<EntryMood> EntryMoods { get; set; }
    }
}