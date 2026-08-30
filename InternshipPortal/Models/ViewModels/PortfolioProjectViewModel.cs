using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InternshipPortal.Models.ViewModels
{
    public class PortfolioProjectViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Project Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1500)]
        [Display(Name = "Project Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Display(Name = "Technologies Used")]
        public string Technologies { get; set; } = string.Empty;

        [Url]
        [StringLength(300)]
        [Display(Name = "Live Project URL")]
        public string? ProjectUrl { get; set; }

        [Url]
        [StringLength(300)]
        [Display(Name = "Repository URL")]
        public string? RepositoryUrl { get; set; }

        [Display(Name = "Project Image")]
        public IFormFile? ImageFile { get; set; }

        public string? CurrentImagePath { get; set; }

        [Display(Name = "Featured Project")]
        public bool IsFeatured { get; set; }
    }
}