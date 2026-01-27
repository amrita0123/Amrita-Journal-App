using System;
using System.Linq;
using System.Threading.Tasks;
using MyDailyJournal.Data;
using MyDailyJournal.Models;
using Microsoft.EntityFrameworkCore;
using MyDailyJournal.Helpers;

namespace MyDailyJournal.MauiBlazor.Services
{
    public class AuthenticationService
    {
        public async Task<bool> IsFirstTimeSetup()
        {
            using var db = new JournalDbContext();
            return !await db.UserSecurity.AnyAsync();
        }
        
        public async Task<bool> CreatePin(string pin)
        {
            try
            {
                using var db = new JournalDbContext();
                
                // Validate PIN format
                if (pin.Length < 4 || pin.Length > 6 || !pin.All(char.IsDigit))
                {
                    return false;
                }
                
                var security = new UserSecurity
                {
                    UserId = "primary_user",
                    PinHash = PasswordHelper.HashPassword(pin), // ← Uses PasswordHelper
                    IsUsingPin = true,
                    AutoLockMinutes = 15
                };
                
                db.UserSecurity.Add(security);
                await db.SaveChangesAsync();
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> ValidatePin(string pin)
        {
            try
            {
                using var db = new JournalDbContext();
                
                var security = await db.UserSecurity
                    .FirstOrDefaultAsync(s => s.UserId == "primary_user");
                
                if (security == null || string.IsNullOrEmpty(security.PinHash))
                {
                    return false;
                }
                
                return PasswordHelper.VerifyPassword(pin, security.PinHash); // ← Uses PasswordHelper
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> ChangePin(string oldPin, string newPin)
        {
            try
            {
                using var db = new JournalDbContext();
                
                var security = await db.UserSecurity
                    .FirstOrDefaultAsync(s => s.UserId == "primary_user");
                
                if (security == null)
                {
                    return false;
                }
                
                // Verify old PIN
                if (!PasswordHelper.VerifyPassword(oldPin, security.PinHash))
                {
                    return false;
                }
                
                // Validate new PIN
                if (newPin.Length < 4 || newPin.Length > 6 || !newPin.All(char.IsDigit))
                {
                    return false;
                }
                
                // Update to new PIN
                security.PinHash = PasswordHelper.HashPassword(newPin);
                security.UpdatedAt = DateTime.Now;
                
                await db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}