using InternshipPortal.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipPortal.Models
{
    public class TrainingHourEntry
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime TrainingDate { get; set; }

        [Range(0.5, 24)]
        [Column(TypeName = "decimal(4,1)")]
        public decimal Hours { get; set; }

        [Required]
        [StringLength(150)]
        public string TaskTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string TaskDescription { get; set; } = string.Empty;

        [StringLength(500)]
        public string? LearnedSkills { get; set; }

        public TrainingHourStatus Status { get; set; }
            = TrainingHourStatus.Pending;

        [StringLength(500)]
        public string? CompanyComment { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        public DateTime? ReviewedAt { get; set; }

        public int TrainingEnrollmentId { get; set; }

        public TrainingEnrollment TrainingEnrollment { get; set; }
            = null!;
    }
}