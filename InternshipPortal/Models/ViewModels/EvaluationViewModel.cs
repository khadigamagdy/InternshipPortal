using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models.ViewModels
{
    public class EvaluationViewModel
    {
        public int InternshipApplicationId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string InternshipTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a rating.")]
        [Range(1, 5, ErrorMessage = "The rating must be between 1 and 5.")]
        [Display(Name = "Student Rating")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please enter your feedback.")]
        [StringLength(
            1000,
            MinimumLength = 10,
            ErrorMessage = "Feedback must be between 10 and 1000 characters.")]
        [Display(Name = "Feedback")]
        public string Feedback { get; set; } = string.Empty;
    }
}