using InternshipPortal.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipPortal.Models
{
    public class StudentPreference
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string Skills { get; set; }
            = string.Empty;

        [StringLength(500)]
        public string? CareerInterests { get; set; }

        [StringLength(100)]
        public string? PreferredLocation { get; set; }

        public WorkMode? PreferredWorkMode { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 1000000)]
        public decimal? MinimumSalary { get; set; }

        public bool AcceptUnpaidInternships { get; set; }

        public bool AcceptRemoteInternships { get; set; }
            = true;

        [Range(1, 24)]
        public int? MaximumWeeklyHours { get; set; }

        public DateTime UpdatedAt { get; set; }
            = DateTime.Now;

        public int StudentId { get; set; }

        public Student Student { get; set; }
            = null!;
    }
}