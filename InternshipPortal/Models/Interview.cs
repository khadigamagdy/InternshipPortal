using InternshipPortal.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models
{
    public class Interview
    {
        public int Id { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }

        [Range(10, 480)]
        public int DurationMinutes { get; set; } = 30;

        public InterviewType Type { get; set; }
            = InterviewType.Online;

        [StringLength(500)]
        [Url]
        public string? MeetingLink { get; set; }

        [StringLength(250)]
        public string? Location { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public InterviewStatus Status { get; set; }
            = InterviewStatus.Pending;

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        public DateTime? RespondedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int InternshipApplicationId { get; set; }

        public InternshipApplication InternshipApplication { get; set; }
            = null!;
    }
}