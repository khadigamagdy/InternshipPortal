using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models.ViewModels
{
    public class ApplicationViewModel
    {
        public int InternshipId { get; set; }

        public string? InternshipTitle { get; set; }

        public string? CompanyName { get; set; }

        [StringLength(1000)]
        [Display(Name = "Cover Letter")]
        public string? CoverLetter { get; set; }

        [Display(Name = "Upload Updated CV")]
        public IFormFile? CVFile { get; set; }

        public string? CurrentCVPath { get; set; }
    }
}