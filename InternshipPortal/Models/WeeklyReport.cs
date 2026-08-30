using InternshipPortal.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class WeeklyReport
    {
        public int Id { get; set; }

        [Range(1, 52)]
        public int WeekNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime WeekStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime WeekEndDate { get; set; }

        [Required]
        [StringLength(2000)]
        public string TasksCompleted { get; set; }
            = string.Empty;

        [Required]
        [StringLength(1000)]
        public string SkillsLearned { get; set; }
            = string.Empty;

        [StringLength(1500)]
        public string? Challenges { get; set; }

        [StringLength(1500)]
        public string? NextWeekPlan { get; set; }

        public WeeklyReportStatus Status { get; set; }
            = WeeklyReportStatus.Draft;

        [StringLength(1000)]
        public string? CompanyFeedback { get; set; }

        [Range(1, 5)]
        public int? CompanyRating { get; set; }

        [StringLength(1000)]
        public string? SupervisorFeedback { get; set; }

        [Range(1, 5)]
        public int? SupervisorRating { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        public DateTime? SubmittedAt { get; set; }

        public DateTime? CompanyReviewedAt { get; set; }

        public DateTime? SupervisorReviewedAt { get; set; }

        public int TrainingEnrollmentId { get; set; }

        public TrainingEnrollment TrainingEnrollment { get; set; }
            = null!;
    }
}