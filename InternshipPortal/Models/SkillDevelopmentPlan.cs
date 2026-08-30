using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class SkillDevelopmentPlan
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        public DateTime? TargetCompletionDate { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int StudentId { get; set; }

        public Student Student { get; set; } = null!;

        public int InternshipId { get; set; }

        public Internship Internship { get; set; } = null!;

        public ICollection<SkillPlanItem> Items { get; set; }
            = new List<SkillPlanItem>();
    }
}