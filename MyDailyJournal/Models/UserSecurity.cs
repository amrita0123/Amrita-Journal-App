using System.ComponentModel.DataAnnotations;
namespace MyDailyJournal.Models
{
    public class UserSecurity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = "primary_user";
        
        public string PinHash { get; set; } // Secret PIN (scrambled for safety)
        
        public string PasswordHash { get; set; } // Or password (scrambled)
        
        public bool IsUsingPin { get; set; } = true; // Using PIN or password?
        
        public int AutoLockMinutes { get; set; } = 15; // Lock after 15 minutes
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}