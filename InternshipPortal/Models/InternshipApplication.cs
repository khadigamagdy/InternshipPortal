using InternshipPortal.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class InternshipApplication
    {
        public int Id { get; set; }

        [StringLength(1000)]
        public string? CoverLetter { get; set; }

        public string? CVPath { get; set; }

        public ApplicationStatus Status { get; set; }
            = ApplicationStatus.Pending;

        public DateTime AppliedAt { get; set; }
            = DateTime.Now;

        public DateTime? ReviewedAt { get; set; }

        public int StudentId { get; set; }

        public Student Student { get; set; }
            = null!;

        public int InternshipId { get; set; }

        public Internship Internship { get; set; }
            = null!;

        public Evaluation? Evaluation { get; set; }
        public TrainingEnrollment? TrainingEnrollment { get; set; }

        public ICollection<Interview> Interviews { get; set; }
            = new List<Interview>();

        public ICollection<ApplicationStatusHistory> StatusHistory { get; set; }
            = new List<ApplicationStatusHistory>();
    }
}