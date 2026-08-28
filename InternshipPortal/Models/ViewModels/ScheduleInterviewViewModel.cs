using InternshipPortal.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models.ViewModels
{
    public class ScheduleInterviewViewModel
    {
        public int InternshipApplicationId { get; set; }

        public string StudentName { get; set; }
            = string.Empty;

        public string InternshipTitle { get; set; }
            = string.Empty;

        [Required(ErrorMessage = "Interview date and time are required.")]
        [Display(Name = "Interview Date and Time")]
        public DateTime ScheduledAt { get; set; }
            = DateTime.Now.AddDays(1);

        [Required]
        [Range(
            10,
            480,
            ErrorMessage = "Duration must be between 10 and 480 minutes.")]
        [Display(Name = "Duration in Minutes")]
        public int DurationMinutes { get; set; }
            = 30;

        [Required]
        [Display(Name = "Interview Type")]
        public InterviewType Type { get; set; }
            = InterviewType.Online;

        [StringLength(500)]
        [Url(ErrorMessage = "Please enter a valid meeting link.")]
        [Display(Name = "Meeting Link")]
        public string? MeetingLink { get; set; }

        [StringLength(250)]
        [Display(Name = "Interview Location")]
        public string? Location { get; set; }

        [StringLength(1000)]
        [Display(Name = "Instructions and Notes")]
        public string? Notes { get; set; }
    }
}