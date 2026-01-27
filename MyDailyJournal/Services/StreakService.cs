using Microsoft.EntityFrameworkCore;

namespace MyDailyJournal.Services
{
    public class StreakService
    {
        // Update streak after writing an entry
        public async Task UpdateStreak(DateTime entryDate)
        {
            using var db = new JournalDbContext();
            
            var tracking = await db.StreakTracking.FirstOrDefaultAsync();
            
            if (tracking == null)
            {
                // First time! Create streak record
                tracking = new Models.StreakTracking
                {
                    CurrentStreak = 1,
                    LongestStreak = 1,
                    LastEntryDate = entryDate,
                    TotalEntries = 1,
                    UpdatedAt = DateTime.Now
                };
                db.StreakTracking.Add(tracking);
            }
            else
            {
                // Calculate days between last entry and this one
                if (tracking.LastEntryDate.HasValue)
                {
                    var daysDiff = (entryDate.Date - tracking.LastEntryDate.Value.Date).Days;
                    
                    if (daysDiff == 1)
                    {
                        // Yay! Consecutive day - add to streak!
                        tracking.CurrentStreak++;
                        
                        // Is this your best streak ever?
                        if (tracking.CurrentStreak > tracking.LongestStreak)
                        {
                            tracking.LongestStreak = tracking.CurrentStreak;
                        }
                    }
                    else if (daysDiff > 1)
                    {
                        // Oh no! You missed a day. Start over.
                        tracking.CurrentStreak = 1;
                    }
                    // If daysDiff == 0, same day entry (update), don't change streak
                }
                else
                {
                    tracking.CurrentStreak = 1;
                }
                
                tracking.LastEntryDate = entryDate;
                tracking.TotalEntries++;
                tracking.UpdatedAt = DateTime.Now;
            }
            
            await db.SaveChangesAsync();
        }
        
        // Get current streak info
        public async Task<(int current, int longest, int total)> GetStreakInfo()
        {
            using var db = new JournalDbContext();
            
            var tracking = await db.StreakTracking.FirstOrDefaultAsync();
            
            if (tracking == null)
            {
                return (0, 0, 0);
            }
            
            // Check if streak is still active
            if (tracking.LastEntryDate.HasValue)
            {
                var daysSinceLastEntry = (DateTime.Now.Date - tracking.LastEntryDate.Value.Date).Days;
                
                // If more than 1 day since last entry, streak is broken
                if (daysSinceLastEntry > 1)
                {
                    tracking.CurrentStreak = 0;
                    await db.SaveChangesAsync();
                }
            }
            
            return (tracking.CurrentStreak, tracking.LongestStreak, tracking.TotalEntries);
        }
        
        // Get list of days you missed
        public async Task<List<DateTime>> GetMissedDays(DateTime startDate, DateTime endDate)
        {
            using var db = new JournalDbContext();
            
            // Get all dates with entries
            var datesWithEntries = await db.JournalEntries
                .Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate)
                .Select(e => e.EntryDate.Date)
                .ToListAsync();
            
            // Find missing days
            var missedDays = new List<DateTime>();
            
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (!datesWithEntries.Contains(date))
                {
                    missedDays.Add(date);
                }
            }
            
            return missedDays;
        }
    }
}