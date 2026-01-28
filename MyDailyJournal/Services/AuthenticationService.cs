using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyDailyJournal.Data;
using MyDailyJournal.Models;
using MyDailyJournal.Helpers;

namespace MyDailyJournal.MauiBlazor.Services
{
    public class AuthenticationService
    {
        private readonly JournalDbContext _db;

        // ✅ Inject DbContext
        public AuthenticationService(JournalDbContext db)
        {
            _db = db;
        }

        // Check if app is being set up for the first time
        public async Task<bool> IsFirstTimeSetup()
        {
            return !await _db.UserSecurity.AnyAsync();
        }

        // Create a new PIN
        public async Task<bool> CreatePin(string pin)
        {
            try
            {
                // Validate PIN format
                if (pin.Length < 4 || pin.Length > 6 || !pin.All(char.IsDigit))
                    return false;

                var security = new UserSecurity
                {
                    UserId = "primary_user",
                    PinHash = PasswordHelper.HashPassword(pin),
                    IsUsingPin = true,
                    AutoLockMinutes = 15,
                    UpdatedAt = DateTime.Now
                };

                _db.UserSecurity.Add(security);
                await _db.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Validate an existing PIN
        public async Task<bool> ValidatePin(string pin)
        {
            try
            {
                var security = await _db.UserSecurity
                    .FirstOrDefaultAsync(s => s.UserId == "primary_user");

                if (security == null || string.IsNullOrEmpty(security.PinHash))
                    return false;

                return PasswordHelper.VerifyPassword(pin, security.PinHash);
            }
            catch
            {
                return false;
            }
        }

        // Change the PIN
        public async Task<bool> ChangePin(string oldPin, string newPin)
        {
            try
            {
                var security = await _db.UserSecurity
                    .FirstOrDefaultAsync(s => s.UserId == "primary_user");

                if (security == null)
                    return false;

                // Verify old PIN
                if (!PasswordHelper.VerifyPassword(oldPin, security.PinHash))
                    return false;

                // Validate new PIN
                if (newPin.Length < 4 || newPin.Length > 6 || !newPin.All(char.IsDigit))
                    return false;

                // Update to new PIN
                security.PinHash = PasswordHelper.HashPassword(newPin);
                security.UpdatedAt = DateTime.Now;

                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<List<JournalEntry>> GetEntries(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _db.JournalEntries.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.EntryDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.EntryDate <= endDate.Value);

            return await query.OrderByDescending(e => e.EntryDate).ToListAsync();
        }
    }
}
