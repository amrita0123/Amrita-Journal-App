using Microsoft.EntityFrameworkCore;
using MyDailyJournal.Data;
using MyDailyJournal.Models;

namespace MyDailyJournal.Services
{
    public class StreakService
    {
        private readonly JournalDbContext _db;

        // ✅ Inject the DbContext
        public StreakService(JournalDbContext db)
        {
            _db = db;
        }

        // Update streak after writing an entry
        public async Task UpdateStreak(DateTime entryDate)
        {
            var tracking = await _db.StreakTracking.FirstOrDefaultAsync();

            if (tracking == null)
            {
                // First time! Create streak record
                tracking = new StreakTracking
                {
                    CurrentStreak = 1,
                    LongestStreak = 1,
                    LastEntryDate = entryDate,
                    TotalEntries = 1,
                    UpdatedAt = DateTime.Now
                };
                _db.StreakTracking.Add(tracking);
            }
            else
            {
                if (tracking.LastEntryDate.HasValue)
                {
                    var daysDiff = (entryDate.Date - tracking.LastEntryDate.Value.Date).Days;

                    if (daysDiff == 1)
                    {
                        // Consecutive day
                        tracking.CurrentStreak++;

                        if (tracking.CurrentStreak > tracking.LongestStreak)
                            tracking.LongestStreak = tracking.CurrentStreak;
                    }
                    else if (daysDiff > 1)
                    {
                        // Missed day(s)
                        tracking.CurrentStreak = 1;
                    }
                    // daysDiff == 0 -> same-day update, do nothing
                }
                else
                {
                    tracking.CurrentStreak = 1;
                }

                tracking.LastEntryDate = entryDate;
                tracking.TotalEntries++;
                tracking.UpdatedAt = DateTime.Now;
            }

            await _db.SaveChangesAsync();
        }

        // Get current streak info
        public async Task<(int current, int longest, int total)> GetStreakInfo()
        {
            var tracking = await _db.StreakTracking.FirstOrDefaultAsync();

            if (tracking == null) return (0, 0, 0);

            if (tracking.LastEntryDate.HasValue)
            {
                var daysSinceLastEntry = (DateTime.Now.Date - tracking.LastEntryDate.Value.Date).Days;

                if (daysSinceLastEntry > 1)
                {
                    tracking.CurrentStreak = 0;
                    await _db.SaveChangesAsync();
                }
            }

            return (tracking.CurrentStreak, tracking.LongestStreak, tracking.TotalEntries);
        }

        // Get list of days you missed
        public async Task<List<DateTime>> GetMissedDays(DateTime startDate, DateTime endDate)
        {
            var datesWithEntries = await _db.JournalEntries
                .Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate)
                .Select(e => e.EntryDate.Date)
                .ToListAsync();

            var missedDays = new List<DateTime>();

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (!datesWithEntries.Contains(date))
                    missedDays.Add(date);
            }

            return missedDays;
        }
    }
}
