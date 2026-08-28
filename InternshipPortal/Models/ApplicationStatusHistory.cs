using InternshipPortal.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class ApplicationStatusHistory
    {
        public int Id { get; set; }

        public ApplicationStatus? PreviousStatus { get; set; }

        public ApplicationStatus NewStatus { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        public DateTime ChangedAt { get; set; }
            = DateTime.Now;

        public int InternshipApplicationId { get; set; }

        public InternshipApplication InternshipApplication { get; set; }
            = null!;

        public string? ChangedByUserId { get; set; }

        public IdentityUser? ChangedByUser { get; set; }
    }
}