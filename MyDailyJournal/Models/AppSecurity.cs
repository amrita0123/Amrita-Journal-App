using System.ComponentModel.DataAnnotations;

namespace MyDailyJournal.Models
{
    public class AppSecurity
    {
        [Key]
        public int Id { get; set; }

        public string PinHash { get; set; }   // Stored securely
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}