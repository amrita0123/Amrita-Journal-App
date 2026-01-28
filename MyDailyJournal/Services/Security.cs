using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MyDailyJournal.Models;

namespace MyDailyJournal.Services
{
    public class SecurityService
    {
        public async Task<bool> IsPinSet()
        {
            using var db = new JournalDbContext();
            return await db.AppSecurity.AnyAsync();
        }

        public async Task SetPin(string pin)
        {
            using var db = new JournalDbContext();

            var hash = HashPin(pin);

            db.AppSecurity.Add(new AppSecurity
            {
                PinHash = hash
            });

            await db.SaveChangesAsync();
        }

        public async Task<bool> VerifyPin(string pin)
        {
            using var db = new JournalDbContext();

            var record = await db.AppSecurity.FirstOrDefaultAsync();
            if (record == null) return false;

            return record.PinHash == HashPin(pin);
        }

        private string HashPin(string pin)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(pin));
            return Convert.ToBase64String(bytes);
        }
    }
}