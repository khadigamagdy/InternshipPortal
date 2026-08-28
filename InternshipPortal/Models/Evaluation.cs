using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class Evaluation
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string Feedback { get; set; }

        public DateTime EvaluationDate { get; set; } = DateTime.Now;

        public int InternshipApplicationId { get; set; }

        public InternshipApplication InternshipApplication { get; set; }
    }
}