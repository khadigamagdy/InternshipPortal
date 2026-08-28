using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class SavedInternship
    {
        public int Id { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.Now;

        [Required]
        public int StudentId { get; set; }

        public Student Student { get; set; } = null!;

        [Required]
        public int InternshipId { get; set; }

        public Internship Internship { get; set; } = null!;
    }
}