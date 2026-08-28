using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(100)]
        public string University { get; set; }

        [Required]
        [StringLength(100)]
        public string Faculty { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; }

        [Range(2020, 2040)]
        public int GraduationYear { get; set; }

        [StringLength(500)]
        public string? Skills { get; set; }

        public string? CVPath { get; set; }

        [Required]
        public string UserId { get; set; }

        public IdentityUser User { get; set; }

        public ICollection<InternshipApplication> Applications { get; set; }
            = new List<InternshipApplication>();
    }
}