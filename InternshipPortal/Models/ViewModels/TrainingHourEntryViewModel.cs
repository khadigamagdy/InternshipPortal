using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models.ViewModels
{
    public class TrainingHourEntryViewModel
    {
        public int TrainingEnrollmentId { get; set; }

        public string InternshipTitle { get; set; }
            = string.Empty;

        public string CompanyName { get; set; }
            = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Training Date")]
        public DateTime TrainingDate { get; set; }
            = DateTime.Today;

        [Required]
        [Range(
            0.5,
            24,
            ErrorMessage =
                "Hours must be between 0.5 and 24.")]
        public decimal Hours { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Task Title")]
        public string TaskTitle { get; set; }
            = string.Empty;

        [Required]
        [StringLength(1000)]
        [Display(Name = "Task Description")]
        public string TaskDescription { get; set; }
            = string.Empty;

        [StringLength(500)]
        [Display(Name = "Skills Learned")]
        public string? LearnedSkills { get; set; }
    }
}