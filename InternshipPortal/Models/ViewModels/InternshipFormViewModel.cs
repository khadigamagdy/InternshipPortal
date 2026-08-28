using InternshipPortal.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models.ViewModels
{
    public class InternshipFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Required Skills")]
        public string RequiredSkills { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [Display(Name = "Work Mode")]
        public WorkMode WorkMode { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Application Deadline")]
        public DateTime ApplicationDeadline { get; set; }

        [Range(1, 1000)]
        [Display(Name = "Available Positions")]
        public int AvailablePositions { get; set; }

        [Display(Name = "Paid Internship")]
        public bool IsPaid { get; set; }

        [Range(0, 1000000)]
        public decimal? Salary { get; set; }
    }
}