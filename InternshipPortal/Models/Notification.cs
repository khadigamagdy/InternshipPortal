using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; }

        public IdentityUser User { get; set; }
    }
}