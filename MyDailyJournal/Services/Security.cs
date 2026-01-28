using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MyDailyJournal.Data;
using MyDailyJournal.Models;

namespace MyDailyJournal.Services
{
    public class SecurityService
    {
        private readonly JournalDbContext _db;

        // ✅ Inject the DbContext
        public SecurityService(JournalDbContext db)
        {
            _db = db;
        }

        // Check if a PIN has been set
        public async Task<bool> IsPinSet()
        {
            return await _db.AppSecurity.AnyAsync();
        }

        // Set a new PIN
        public async Task SetPin(string pin)
        {
            var hash = HashPin(pin);

            _db.AppSecurity.Add(new AppSecurity
            {
                PinHash = hash
            });

            await _db.SaveChangesAsync();
        }

        // Verify PIN
        public async Task<bool> VerifyPin(string pin)
        {
            var record = await _db.AppSecurity.FirstOrDefaultAsync();
            if (record == null) return false;

            return record.PinHash == HashPin(pin);
        }

        // Hashing helper
        private string HashPin(string pin)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(pin));
            return Convert.ToBase64String(bytes);
        }
    }
}