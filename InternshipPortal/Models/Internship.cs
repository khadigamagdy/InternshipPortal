using InternshipPortal.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipPortal.Models
{
    public class Internship
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [StringLength(500)]
        public string RequiredSkills { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        public WorkMode WorkMode { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ApplicationDeadline { get; set; }

        [Range(1, 1000)]
        public int AvailablePositions { get; set; }

        public bool IsPaid { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Salary { get; set; }

        public bool IsApproved { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int CompanyId { get; set; }

        public Company Company { get; set; }

        public ICollection<InternshipApplication> Applications { get; set; }
            = new List<InternshipApplication>();
    }
}