using System.ComponentModel.DataAnnotations;
using MyDailyJournal.Models;

namespace MyDailyJournal.Models
{
    public class JournalEntry
    {
        [Key] public int Id { get; set; }

        [Required] public DateTime EntryDate { get; set; } // What day is this entry for?

        [Required] public string Title { get; set; } // "My Amazing Day!"

        [Required] public string Content { get; set; } // What you wrote

        public int WordCount { get; set; } = 0; // How many words did you write?

        public int? CategoryId { get; set; } // Which category?
        public virtual Category Category { get; set; }

        public int PrimaryMoodId { get; set; } // Your main feeling
        public virtual Mood PrimaryMood { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // All moods for this entry (primary + up to 2 secondary)
        public virtual ICollection<EntryMood> EntryMoods { get; set; }

        // All tags for this entry
        public virtual ICollection<EntryTag> EntryTags { get; set; }
    }
}


