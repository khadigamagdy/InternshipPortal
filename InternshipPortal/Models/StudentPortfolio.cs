using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class StudentPortfolio
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Headline { get; set; } = string.Empty;

        [Required]
        [StringLength(1500)]
        public string Bio { get; set; } = string.Empty;

        [StringLength(800)]
        public string? SkillsSummary { get; set; }

        [Url]
        [StringLength(300)]
        public string? GitHubUrl { get; set; }

        [Url]
        [StringLength(300)]
        public string? LinkedInUrl { get; set; }

        [Url]
        [StringLength(300)]
        public string? PersonalWebsiteUrl { get; set; }

        [Required]
        [StringLength(180)]
        public string PortfolioSlug { get; set; } = string.Empty;

        public bool IsPublic { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int StudentId { get; set; }

        public Student Student { get; set; } = null!;

        public ICollection<PortfolioProject> Projects { get; set; }
            = new List<PortfolioProject>();
    }
}