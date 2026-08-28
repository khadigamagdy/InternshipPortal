using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [Url]
        public string? Website { get; set; }

        public string? LogoPath { get; set; }

        [Required]
        public string UserId { get; set; }

        public IdentityUser User { get; set; }

        public ICollection<Internship> Internships { get; set; }
            = new List<Internship>();
    }
}