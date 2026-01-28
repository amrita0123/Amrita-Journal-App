
using MyDailyJournal.Models;
using Microsoft.EntityFrameworkCore;

namespace MyDailyJournal.Services
{
    public class JournalService
    {
        private readonly StreakService _streakService = new StreakService();
        
        // Check if there's already an entry for today
        public async Task<bool> HasEntryForDate(DateTime date)
        {
            using var db = new JournalDbContext();
            return await db.JournalEntries
                .AnyAsync(e => e.EntryDate.Date == date.Date);
        }
        
        // Get today's entry (if it exists)
        public async Task<JournalEntry> GetEntryForDate(DateTime date)
        {
            using var db = new JournalDbContext();
            
            return await db.JournalEntries
                .Include(e => e.Category)
                .Include(e => e.PrimaryMood)
                .Include(e => e.EntryMoods)
                    .ThenInclude(em => em.Mood)
                .Include(e => e.EntryTags)
                    .ThenInclude(et => et.Tag)
                .FirstOrDefaultAsync(e => e.EntryDate.Date == date.Date);
        }
        
        // Create a new journal entry
        public async Task<JournalEntry> CreateEntry(
            DateTime date,
            string title,
            string content,
            int primaryMoodId,
            List<int> secondaryMoodIds,
            int? categoryId,
            List<int> tagIds)
        {
            using var db = new JournalDbContext();
            
            // Make sure there's no entry for this date already
            if (await HasEntryForDate(date))
            {
                throw new InvalidOperationException("An entry already exists for this date!");
            }
            
            // Count words
            var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            
            // Create the entry
            var entry = new JournalEntry
            {
                EntryDate = date.Date,
                Title = title,
                Content = content,
                WordCount = wordCount,
                CategoryId = categoryId,
                PrimaryMoodId = primaryMoodId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            
            db.JournalEntries.Add(entry);
            await db.SaveChangesAsync();
            
            // Add primary mood
            db.EntryMoods.Add(new EntryMood
            {
                JournalEntryId = entry.Id,
                MoodId = primaryMoodId,
                IsPrimary = true
            });
            
            // Add secondary moods (max 2)
            if (secondaryMoodIds != null && secondaryMoodIds.Any())
            {
                foreach (var moodId in secondaryMoodIds.Take(2))
                {
                    db.EntryMoods.Add(new EntryMood
                    {
                        JournalEntryId = entry.Id,
                        MoodId = moodId,
                        IsPrimary = false
                    });
                }
            }
            
            // Add tags
            if (tagIds != null && tagIds.Any())
            {
                foreach (var tagId in tagIds)
                {
                    db.EntryTags.Add(new EntryTag
                    {
                        JournalEntryId = entry.Id,
                        TagId = tagId
                    });
                    
                    // Update tag usage count
                    var tag = await db.Tags.FindAsync(tagId);
                    if (tag != null)
                    {
                        tag.UsageCount++;
                    }
                }
            }
            
            await db.SaveChangesAsync();
            
            // Update streak!
            await _streakService.UpdateStreak(date);
            
            return entry;
        }
        
        // Update an existing entry
        public async Task<JournalEntry> UpdateEntry(
            int entryId,
            string title,
            string content,
            int primaryMoodId,
            List<int> secondaryMoodIds,
            int? categoryId,
            List<int> tagIds)
        {
            using var db = new JournalDbContext();
            
            var entry = await db.JournalEntries
                .Include(e => e.EntryMoods)
                .Include(e => e.EntryTags)
                .FirstOrDefaultAsync(e => e.Id == entryId);
            
            if (entry == null)
            {
                throw new InvalidOperationException("Entry not found!");
            }
            
            // Update basic info
            entry.Title = title;
            entry.Content = content;
            entry.WordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            entry.CategoryId = categoryId;
            entry.PrimaryMoodId = primaryMoodId;
            entry.UpdatedAt = DateTime.Now;
            
            // Remove old moods
            db.EntryMoods.RemoveRange(entry.EntryMoods);
            
            // Add new primary mood
            db.EntryMoods.Add(new EntryMood
            {
                JournalEntryId = entry.Id,
                MoodId = primaryMoodId,
                IsPrimary = true
            });
            
            // Add new secondary moods
            if (secondaryMoodIds != null && secondaryMoodIds.Any())
            {
                foreach (var moodId in secondaryMoodIds.Take(2))
                {
                    db.EntryMoods.Add(new EntryMood
                    {
                        JournalEntryId = entry.Id,
                        MoodId = moodId,
                        IsPrimary = false
                    });
                }
            }
            
            // Update tags
            var oldTagIds = entry.EntryTags.Select(et => et.TagId).ToList();
            db.EntryTags.RemoveRange(entry.EntryTags);
            
            // Decrease old tag counts
            foreach (var oldTagId in oldTagIds)
            {
                var tag = await db.Tags.FindAsync(oldTagId);
                if (tag != null && tag.UsageCount > 0)
                {
                    tag.UsageCount--;
                }
            }
            
            // Add new tags and increase counts
            if (tagIds != null && tagIds.Any())
            {
                foreach (var tagId in tagIds)
                {
                    db.EntryTags.Add(new EntryTag
                    {
                        JournalEntryId = entry.Id,
                        TagId = tagId
                    });
                    
                    var tag = await db.Tags.FindAsync(tagId);
                    if (tag != null)
                    {
                        tag.UsageCount++;
                    }
                }
            }
            
            await db.SaveChangesAsync();
            
            return entry;
        }
        
        // Delete an entry
        public async Task DeleteEntry(int entryId)
        {
            using var db = new JournalDbContext();
            
            var entry = await db.JournalEntries
                .Include(e => e.EntryTags)
                .FirstOrDefaultAsync(e => e.Id == entryId);
            
            if (entry == null)
            {
                throw new InvalidOperationException("Entry not found!");
            }
            
            // Decrease tag usage counts
            foreach (var entryTag in entry.EntryTags)
            {
                var tag = await db.Tags.FindAsync(entryTag.TagId);
                if (tag != null && tag.UsageCount > 0)
                {
                    tag.UsageCount--;
                }
            }
            
            db.JournalEntries.Remove(entry);
            await db.SaveChangesAsync();
        }
        
        // Get all entries (with filters)
        public async Task<List<JournalEntry>> GetEntries(
            DateTime? startDate = null,
            DateTime? endDate = null,
            List<int> moodIds = null,
            List<int> tagIds = null,
            string searchText = null)
        {
            using var db = new JournalDbContext();
            
            var query = db.JournalEntries
                .Include(e => e.Category)
                .Include(e => e.PrimaryMood)
                .Include(e => e.EntryMoods)
                    .ThenInclude(em => em.Mood)
                .Include(e => e.EntryTags)
                    .ThenInclude(et => et.Tag)
                .AsQueryable();
            
            // Filter by date range
            if (startDate.HasValue)
            {
                query = query.Where(e => e.EntryDate >= startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                query = query.Where(e => e.EntryDate <= endDate.Value);
            }
            
            // Filter by moods
            if (moodIds != null && moodIds.Any())
            {
                query = query.Where(e => e.EntryMoods.Any(em => moodIds.Contains(em.MoodId)));
            }
            
            // Filter by tags
            if (tagIds != null && tagIds.Any())
            {
                query = query.Where(e => e.EntryTags.Any(et => tagIds.Contains(et.TagId)));
            }
            
            // Search in title and content
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(e => 
                    e.Title.Contains(searchText) || 
                    e.Content.Contains(searchText));
            }
            
            return await query.OrderByDescending(e => e.EntryDate).ToListAsync();
        }
        
        // Get all moods
        public async Task<List<Mood>> GetAllMoods()
        {
            using var db = new JournalDbContext();
            return await db.Moods.OrderBy(m => m.Name).ToListAsync();
        }
        
        // Get all categories
        public async Task<List<Category>> GetAllCategories()
        {
            using var db = new JournalDbContext();
            return await db.Categories.OrderBy(c => c.Name).ToListAsync();
        }
        
        // Get all tags
        public async Task<List<Tag>> GetAllTags()
        {
            using var db = new JournalDbContext();
            return await db.Tags.OrderBy(t => t.Name).ToListAsync();
        }
        
        // Create a custom tag
        public async Task<Tag> CreateCustomTag(string tagName)
        {
            using var db = new JournalDbContext();
            
            // Check if tag already exists
            var existing = await db.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            if (existing != null)
            {
                return existing;
            }
            
            var tag = new Tag
            {
                Name = tagName,
                IsCustom = true,
                UsageCount = 0
            };
            
            db.Tags.Add(tag);
            await db.SaveChangesAsync();
            
            return tag;
        }
        
        public async Task<(List<JournalEntry> entries, int totalCount)> 
            GetPaginatedEntries(int pageNumber, int pageSize)
        {
            using var db = new JournalDbContext();

            var query = db.JournalEntries
                .Include(e => e.PrimaryMood)
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .OrderByDescending(e => e.EntryDate);

            var totalCount = await query.CountAsync();

            var entries = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (entries, totalCount);
        }
        
        public async Task<(List<JournalEntry> entries, int totalCount)>
            SearchAndFilterEntries(
                string searchText,
                DateTime? startDate,
                DateTime? endDate,
                int? moodId,
                List<int> tagIds,
                int pageNumber,
                int pageSize)
        {
            using var db = new JournalDbContext();

            var query = db.JournalEntries
                .Include(e => e.PrimaryMood)
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .AsQueryable();

            // 🔍 Search by title or content
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(e =>
                    e.Title.Contains(searchText) ||
                    e.Content.Contains(searchText));
            }

            // 📅 Date range filter
            if (startDate.HasValue)
                query = query.Where(e => e.EntryDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.EntryDate <= endDate.Value);

            // 😊 Mood filter
            if (moodId.HasValue)
                query = query.Where(e => e.PrimaryMoodId == moodId.Value);

            // 🏷️ Tag filter
            if (tagIds != null && tagIds.Any())
            {
                query = query.Where(e =>
                    e.EntryTags.Any(t => tagIds.Contains(t.TagId)));
            }

            query = query.OrderByDescending(e => e.EntryDate);

            var totalCount = await query.CountAsync();

            var entries = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (entries, totalCount);
        }


    }
}