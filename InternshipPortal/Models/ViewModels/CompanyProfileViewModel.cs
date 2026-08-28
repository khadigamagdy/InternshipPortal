using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models.ViewModels
{
    public class CompanyProfileViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Company Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [Url]
        [Display(Name = "Company Website")]
        public string? Website { get; set; }

        [Display(Name = "Company Logo")]
        public IFormFile? LogoFile { get; set; }

        public string? CurrentLogoPath { get; set; }
    }
}