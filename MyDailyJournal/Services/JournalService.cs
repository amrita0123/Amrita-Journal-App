using Microsoft.EntityFrameworkCore;
using MyDailyJournal.Data;
using MyDailyJournal.Models;

namespace MyDailyJournal.Services
{
    public class JournalService
    {
        private readonly JournalDbContext _db;
        private readonly StreakService _streakService;

        // ✅ Constructor injection for DbContext and StreakService
        public JournalService(JournalDbContext db, StreakService streakService)
        {
            _db = db;
            _streakService = streakService;
        }

        // Check if there's already an entry for a date
        public async Task<bool> HasEntryForDate(DateTime date)
        {
            return await _db.JournalEntries
                .AnyAsync(e => e.EntryDate.Date == date.Date);
        }

        // Get entry for a specific date
        public async Task<JournalEntry> GetEntryForDate(DateTime date)
        {
            return await _db.JournalEntries
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
            if (await HasEntryForDate(date))
                throw new InvalidOperationException("An entry already exists for this date!");

            var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

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

            _db.JournalEntries.Add(entry);
            await _db.SaveChangesAsync();

            // Add primary mood
            _db.EntryMoods.Add(new EntryMood
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
                    _db.EntryMoods.Add(new EntryMood
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
                    _db.EntryTags.Add(new EntryTag
                    {
                        JournalEntryId = entry.Id,
                        TagId = tagId
                    });

                    // Update tag usage count
                    var tag = await _db.Tags.FindAsync(tagId);
                    if (tag != null) tag.UsageCount++;
                }
            }

            await _db.SaveChangesAsync();

            // Update streak using injected service
            await _streakService.UpdateStreak(date);

            return entry;
        }

        // Update existing entry
        public async Task<JournalEntry> UpdateEntry(
            int entryId,
            string title,
            string content,
            int primaryMoodId,
            List<int> secondaryMoodIds,
            int? categoryId,
            List<int> tagIds)
        {
            var entry = await _db.JournalEntries
                .Include(e => e.EntryMoods)
                .Include(e => e.EntryTags)
                .FirstOrDefaultAsync(e => e.Id == entryId);

            if (entry == null) throw new InvalidOperationException("Entry not found!");

            entry.Title = title;
            entry.Content = content;
            entry.WordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            entry.CategoryId = categoryId;
            entry.PrimaryMoodId = primaryMoodId;
            entry.UpdatedAt = DateTime.Now;

            // Remove old moods and tags
            _db.EntryMoods.RemoveRange(entry.EntryMoods);
            var oldTagIds = entry.EntryTags.Select(et => et.TagId).ToList();
            _db.EntryTags.RemoveRange(entry.EntryTags);

            // Decrease old tag counts
            foreach (var oldTagId in oldTagIds)
            {
                var tag = await _db.Tags.FindAsync(oldTagId);
                if (tag != null && tag.UsageCount > 0) tag.UsageCount--;
            }

            // Add new moods
            _db.EntryMoods.Add(new EntryMood
            {
                JournalEntryId = entry.Id,
                MoodId = primaryMoodId,
                IsPrimary = true
            });

            if (secondaryMoodIds != null && secondaryMoodIds.Any())
            {
                foreach (var moodId in secondaryMoodIds.Take(2))
                {
                    _db.EntryMoods.Add(new EntryMood
                    {
                        JournalEntryId = entry.Id,
                        MoodId = moodId,
                        IsPrimary = false
                    });
                }
            }

            // Add new tags and update usage
            if (tagIds != null && tagIds.Any())
            {
                foreach (var tagId in tagIds)
                {
                    _db.EntryTags.Add(new EntryTag
                    {
                        JournalEntryId = entry.Id,
                        TagId = tagId
                    });

                    var tag = await _db.Tags.FindAsync(tagId);
                    if (tag != null) tag.UsageCount++;
                }
            }

            await _db.SaveChangesAsync();
            return entry;
        }

        // Delete entry
        public async Task DeleteEntry(int entryId)
        {
            var entry = await _db.JournalEntries
                .Include(e => e.EntryTags)
                .FirstOrDefaultAsync(e => e.Id == entryId);

            if (entry == null) throw new InvalidOperationException("Entry not found!");

            foreach (var entryTag in entry.EntryTags)
            {
                var tag = await _db.Tags.FindAsync(entryTag.TagId);
                if (tag != null && tag.UsageCount > 0) tag.UsageCount--;
            }

            _db.JournalEntries.Remove(entry);
            await _db.SaveChangesAsync();
        }

        // Get paginated entries
        public async Task<(List<JournalEntry> entries, int totalCount)> GetPaginatedEntries(int pageNumber, int pageSize)
        {
            var query = _db.JournalEntries
                .Include(e => e.PrimaryMood)
                .Include(e => e.EntryTags)
                    .ThenInclude(et => et.Tag)
                .OrderByDescending(e => e.EntryDate);

            var totalCount = await query.CountAsync();
            var entries = await query.Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync();

            return (entries, totalCount);
        }

        // Search & Filter
        public async Task<(List<JournalEntry> entries, int totalCount)> SearchAndFilterEntries(
            string searchText,
            DateTime? startDate,
            DateTime? endDate,
            int? moodId,
            List<int> tagIds,
            int pageNumber,
            int pageSize)
        {
            var query = _db.JournalEntries
                .Include(e => e.PrimaryMood)
                .Include(e => e.EntryTags)
                    .ThenInclude(et => et.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
                query = query.Where(e => e.Title.Contains(searchText) || e.Content.Contains(searchText));

            if (startDate.HasValue)
                query = query.Where(e => e.EntryDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.EntryDate <= endDate.Value);

            if (moodId.HasValue)
                query = query.Where(e => e.PrimaryMoodId == moodId.Value);

            if (tagIds != null && tagIds.Any())
                query = query.Where(e => e.EntryTags.Any(et => tagIds.Contains(et.TagId)));

            var totalCount = await query.CountAsync();
            var entries = await query.OrderByDescending(e => e.EntryDate)
                                     .Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync();

            return (entries, totalCount);
        }

        // Get all moods
        public async Task<List<Mood>> GetAllMoods() => await _db.Moods.OrderBy(m => m.Name).ToListAsync();

        // Get all categories
        public async Task<List<Category>> GetAllCategories() => await _db.Categories.OrderBy(c => c.Name).ToListAsync();

        // Get all tags
        public async Task<List<Tag>> GetAllTags() => await _db.Tags.OrderBy(t => t.Name).ToListAsync();

        // Create a custom tag
        public async Task<Tag> CreateCustomTag(string tagName)
        {
            var existing = await _db.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            if (existing != null) return existing;

            var tag = new Tag { Name = tagName, IsCustom = true, UsageCount = 0 };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();
            return tag;
        }

        public async Task<JournalEntry> GetEntryById(int? entryId)
        {
            throw new NotImplementedException();
        }
        public async Task<List<JournalEntry>> GetEntries(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _db.JournalEntries
                .Include(e => e.PrimaryMood)
                .Include(e => e.EntryTags)
                .ThenInclude(et => et.Tag)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.EntryDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.EntryDate <= endDate.Value);

            return await query.OrderByDescending(e => e.EntryDate).ToListAsync();
        }

    }
}
