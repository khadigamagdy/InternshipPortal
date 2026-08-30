using InternshipPortal.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class SkillPlanItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SkillName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? LearningGoal { get; set; }

        [StringLength(500)]
        public string? LearningResourceUrl { get; set; }

        public SkillProgressStatus Status { get; set; }
            = SkillProgressStatus.NotStarted;

        [Range(0, 100)]
        public int ProgressPercentage { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int DisplayOrder { get; set; }

        public int SkillDevelopmentPlanId { get; set; }

        public SkillDevelopmentPlan SkillDevelopmentPlan { get; set; }
            = null!;
    }
}