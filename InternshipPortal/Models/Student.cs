using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string University { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Faculty { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [Range(2020, 2040)]
        public int GraduationYear { get; set; }

        public string? CVPath { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public IdentityUser User { get; set; } = null!;

        public StudentPreference? Preference { get; set; }

        public StudentPortfolio? Portfolio { get; set; }

        public ICollection<InternshipApplication> Applications { get; set; }
            = new List<InternshipApplication>();

        public ICollection<SavedInternship> SavedInternships { get; set; }
            = new List<SavedInternship>();

        public ICollection<SkillDevelopmentPlan> SkillDevelopmentPlans { get; set; }
            = new List<SkillDevelopmentPlan>();
    }
}