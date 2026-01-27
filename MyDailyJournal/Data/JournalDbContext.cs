using Microsoft.EntityFrameworkCore;
using MyDailyJournal.Data;
using MyDailyJournal.Models;

    public class JournalDbContext : DbContext
    {
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<Mood> Moods { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<EntryMood> EntryMoods { get; set; }
        public DbSet<EntryTag> EntryTags { get; set; }
        public DbSet<UserSecurity> UserSecurity { get; set; }
        public DbSet<StreakTracking> StreakTracking { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Use platform-specific path
            var dbPath = DatabasePath.GetDatabasePath();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Same configuration as WPF
            modelBuilder.Entity<JournalEntry>()
                .HasIndex(e => e.EntryDate)
                .IsUnique();
            
            modelBuilder.Entity<Mood>()
                .HasIndex(m => m.Name)
                .IsUnique();
            
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();
            
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();
            
            modelBuilder.Entity<EntryMood>()
                .HasOne(em => em.JournalEntry)
                .WithMany(je => je.EntryMoods)
                .HasForeignKey(em => em.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<EntryTag>()
                .HasOne(et => et.JournalEntry)
                .WithMany(je => je.EntryTags)
                .HasForeignKey(et => et.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);
            
            SeedData(modelBuilder);
        }
        
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Moods
            var moods = new List<Mood>
            {
                new Mood { Id = 1, Name = "Happy", Type = "Positive", Emoji = "😊", ColorCode = "#FFD700" },
                new Mood { Id = 2, Name = "Excited", Type = "Positive", Emoji = "🤩", ColorCode = "#FF6B6B" },
                new Mood { Id = 3, Name = "Relaxed", Type = "Positive", Emoji = "😌", ColorCode = "#4ECDC4" },
                new Mood { Id = 4, Name = "Grateful", Type = "Positive", Emoji = "🙏", ColorCode = "#95E1D3" },
                new Mood { Id = 5, Name = "Confident", Type = "Positive", Emoji = "💪", ColorCode = "#F38181" },
                new Mood { Id = 6, Name = "Calm", Type = "Neutral", Emoji = "😐", ColorCode = "#A8E6CF" },
                new Mood { Id = 7, Name = "Thoughtful", Type = "Neutral", Emoji = "🤔", ColorCode = "#FFE156" },
                new Mood { Id = 8, Name = "Curious", Type = "Neutral", Emoji = "🧐", ColorCode = "#FF8B94" },
                new Mood { Id = 9, Name = "Nostalgic", Type = "Neutral", Emoji = "🥺", ColorCode = "#C7CEEA" },
                new Mood { Id = 10, Name = "Bored", Type = "Neutral", Emoji = "😑", ColorCode = "#B5B5B5" },
                new Mood { Id = 11, Name = "Sad", Type = "Negative", Emoji = "😢", ColorCode = "#6C757D" },
                new Mood { Id = 12, Name = "Angry", Type = "Negative", Emoji = "😠", ColorCode = "#DC3545" },
                new Mood { Id = 13, Name = "Stressed", Type = "Negative", Emoji = "😰", ColorCode = "#FFC107" },
                new Mood { Id = 14, Name = "Lonely", Type = "Negative", Emoji = "😔", ColorCode = "#6C5B7B" },
                new Mood { Id = 15, Name = "Anxious", Type = "Negative", Emoji = "😨", ColorCode = "#C06C84" }
            };
            modelBuilder.Entity<Mood>().HasData(moods);
            
            // Seed Categories
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "School", ColorCode = "#3498DB", IconName = "School" },
                new Category { Id = 2, Name = "Family", ColorCode = "#E74C3C", IconName = "Home" },
                new Category { Id = 3, Name = "Friends", ColorCode = "#9B59B6", IconName = "People" },
                new Category { Id = 4, Name = "Personal", ColorCode = "#1ABC9C", IconName = "Person" },
                new Category { Id = 5, Name = "Hobbies", ColorCode = "#F39C12", IconName = "Sports" },
                new Category { Id = 6, Name = "Travel", ColorCode = "#16A085", IconName = "Flight" }
            };
            modelBuilder.Entity<Category>().HasData(categories);
            
            // Seed Tags
            var tags = new List<Tag>();
            string[] tagNames = { "Work", "Career", "Studies", "Family", "Friends", "Relationships",
                "Health", "Fitness", "Personal Growth", "Self-care", "Hobbies", "Travel", "Nature",
                "Finance", "Spirituality", "Birthday", "Holiday", "Vacation", "Celebration",
                "Exercise", "Reading", "Writing", "Cooking", "Meditation", "Yoga", "Music",
                "Shopping", "Parenting", "Projects", "Planning", "Reflection" };
            
            for (int i = 0; i < tagNames.Length; i++)
            {
                tags.Add(new Tag { Id = i + 1, Name = tagNames[i], IsCustom = false });
            }
            modelBuilder.Entity<Tag>().HasData(tags);
            
            // Seed StreakTracking
            modelBuilder.Entity<StreakTracking>().HasData(
                new StreakTracking { Id = 1, CurrentStreak = 0, LongestStreak = 0, TotalEntries = 0 }
            );
        }
    }
    