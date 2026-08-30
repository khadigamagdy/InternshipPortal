using InternshipPortal.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class TrainingEnrollment
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ExpectedEndDate { get; set; }

        [Range(20, 2000)]
        public int RequiredHours { get; set; } = 120;

        public TrainingStatus Status { get; set; }
            = TrainingStatus.PendingUniversityApproval;

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        public DateTime? UniversityApprovedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [StringLength(100)]
        public string? CompanyMentorName { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string? CompanyMentorEmail { get; set; }

        public int InternshipApplicationId { get; set; }

        public InternshipApplication InternshipApplication { get; set; }
            = null!;

        public string? UniversitySupervisorUserId { get; set; }

        public IdentityUser? UniversitySupervisorUser { get; set; }

        public ICollection<TrainingHourEntry> HourEntries { get; set; }
            = new List<TrainingHourEntry>();

        public ICollection<WeeklyReport> WeeklyReports { get; set; }
            = new List<WeeklyReport>();
    }
}