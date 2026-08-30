using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class PortfolioProject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Technologies { get; set; } = string.Empty;

        [Url]
        [StringLength(300)]
        public string? ProjectUrl { get; set; }

        [Url]
        [StringLength(300)]
        public string? RepositoryUrl { get; set; }

        [StringLength(300)]
        public string? ImagePath { get; set; }

        public bool IsFeatured { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int StudentPortfolioId { get; set; }

        public StudentPortfolio StudentPortfolio { get; set; } = null!;
    }
}