using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models.ViewModels
{
    public class WeeklyReportViewModel
    {
        public int TrainingEnrollmentId { get; set; }

        public string InternshipTitle { get; set; }
            = string.Empty;

        public string CompanyName { get; set; }
            = string.Empty;

        [Range(1, 52)]
        [Display(Name = "Week Number")]
        public int WeekNumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Week Start Date")]
        public DateTime WeekStartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Week End Date")]
        public DateTime WeekEndDate { get; set; }

        [Required]
        [StringLength(2000)]
        [Display(Name = "Tasks Completed")]
        public string TasksCompleted { get; set; }
            = string.Empty;

        [Required]
        [StringLength(1000)]
        [Display(Name = "Skills Learned")]
        public string SkillsLearned { get; set; }
            = string.Empty;

        [StringLength(1500)]
        public string? Challenges { get; set; }

        [StringLength(1500)]
        [Display(Name = "Next Week Plan")]
        public string? NextWeekPlan { get; set; }
    }
}