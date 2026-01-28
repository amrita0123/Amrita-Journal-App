using Microsoft.EntityFrameworkCore;
using MyDailyJournal.Data;
using MyDailyJournal.Models;

namespace MyDailyJournal.Services
{
    public class MoodService
    {
        private readonly JournalDbContext _db;

        // ✅ Inject the DbContext
        public MoodService(JournalDbContext db)
        {
            _db = db;
        }

        // Get all moods
        public async Task<List<Mood>> GetAllMoods()
        {
            return await _db.Moods
                .OrderBy(m => m.Name)
                .ToListAsync();
        }
    }
}