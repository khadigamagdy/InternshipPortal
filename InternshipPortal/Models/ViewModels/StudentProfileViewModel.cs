using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models.ViewModels
{
    public class StudentProfileViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [StringLength(100)]
        public string University { get; set; }

        [Required]
        [StringLength(100)]
        public string Faculty { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; }

        [Required]
        [Range(2020, 2040)]
        [Display(Name = "Graduation Year")]
        public int GraduationYear { get; set; }

        [Display(Name = "Upload CV")]
        public IFormFile? CVFile { get; set; }

        public string? CurrentCVPath { get; set; }
    }
}