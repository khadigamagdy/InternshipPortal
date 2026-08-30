using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models.ViewModels
{
    public class TalentSearchViewModel
    {
        [StringLength(100)]
        [Display(Name = "Search")]
        public string? Search { get; set; }

        [StringLength(100)]
        [Display(Name = "Specialization")]
        public string? Specialization { get; set; }

        [StringLength(100)]
        [Display(Name = "University")]
        public string? University { get; set; }

        [StringLength(100)]
        [Display(Name = "Required Skill")]
        public string? Skill { get; set; }

        [Range(2020, 2040)]
        [Display(Name = "Graduation Year")]
        public int? GraduationYear { get; set; }

        [Range(0, 5)]
        [Display(Name = "Minimum Rating")]
        public double? MinimumRating { get; set; }

        [Range(0, 5000)]
        [Display(Name = "Minimum Verified Hours")]
        public int? MinimumVerifiedHours { get; set; }

        [Display(Name = "Sort By")]
        public string SortBy { get; set; } = "score";

        public List<string> AvailableSpecializations { get; set; }
            = new List<string>();

        public List<string> AvailableUniversities { get; set; }
            = new List<string>();

        public List<TalentCandidateViewModel> Candidates { get; set; }
            = new List<TalentCandidateViewModel>();

        public int TotalPublicProfiles { get; set; }

        public int VerifiedCandidates { get; set; }

        public int TotalProjects { get; set; }

        public double PlatformAverageRating { get; set; }
    }

    public class TalentCandidateViewModel
    {
        public int StudentId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string University { get; set; } = string.Empty;

        public string Faculty { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;

        public int GraduationYear { get; set; }

        public string Headline { get; set; } = string.Empty;

        public string? SkillsSummary { get; set; }

        public string PortfolioSlug { get; set; } = string.Empty;

        public int ProjectsCount { get; set; }

        public int CompletedTrainingsCount { get; set; }

        public int VerifiedTrainingHours { get; set; }

        public int EvaluationsCount { get; set; }

        public double AverageRating { get; set; }

        public int TalentScore { get; set; }

        public bool HasVerifiedExperience { get; set; }

        public List<string> Skills { get; set; }
            = new List<string>();
    }
}