using Microsoft.EntityFrameworkCore;
using MyDailyJournal.Models;

namespace MyDailyJournal.Services
{
    public class MoodService
    {
        public async Task<List<Mood>> GetAllMoods()
        {
            using var db = new JournalDbContext();

            return await db.Moods
                .OrderBy(m => m.Name)
                .ToListAsync();
        }
    }
}