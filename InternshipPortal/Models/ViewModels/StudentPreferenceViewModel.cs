using InternshipPortal.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models.ViewModels
{
    public class StudentPreferenceViewModel
    {
        [Required(ErrorMessage = "Please enter your skills.")]
        [StringLength(1000)]
        [Display(Name = "Your Skills")]
        public string Skills { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Career Interests")]
        public string? CareerInterests { get; set; }

        [StringLength(100)]
        [Display(Name = "Preferred Location")]
        public string? PreferredLocation { get; set; }

        [Display(Name = "Preferred Work Mode")]
        public WorkMode? PreferredWorkMode { get; set; }

        [Range(0, 1000000)]
        [Display(Name = "Minimum Expected Salary")]
        public decimal? MinimumSalary { get; set; }

        [Display(Name = "I accept unpaid internships")]
        public bool AcceptUnpaidInternships { get; set; }

        [Display(Name = "I accept remote internships")]
        public bool AcceptRemoteInternships { get; set; } = true;

        [Range(
            1,
            24,
            ErrorMessage = "Maximum weekly hours must be between 1 and 24.")]
        [Display(Name = "Maximum Weekly Hours")]
        public int? MaximumWeeklyHours { get; set; }
    }
}