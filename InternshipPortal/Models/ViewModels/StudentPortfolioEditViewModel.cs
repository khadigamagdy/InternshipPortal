using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models.ViewModels
{
    public class StudentPortfolioEditViewModel
    {
        [Required]
        [StringLength(150)]
        [Display(Name = "Professional Headline")]
        public string Headline { get; set; } = string.Empty;

        [Required]
        [StringLength(1500)]
        [Display(Name = "About Me")]
        public string Bio { get; set; } = string.Empty;

        [StringLength(800)]
        [Display(Name = "Skills")]
        public string? SkillsSummary { get; set; }

        [Url]
        [StringLength(300)]
        [Display(Name = "GitHub Profile")]
        public string? GitHubUrl { get; set; }

        [Url]
        [StringLength(300)]
        [Display(Name = "LinkedIn Profile")]
        public string? LinkedInUrl { get; set; }

        [Url]
        [StringLength(300)]
        [Display(Name = "Personal Website")]
        public string? PersonalWebsiteUrl { get; set; }

        [Display(Name = "Make Portfolio Public")]
        public bool IsPublic { get; set; } = true;
    }
}