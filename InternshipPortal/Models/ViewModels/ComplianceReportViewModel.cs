using System.ComponentModel.DataAnnotations;

namespace InternshipPortal.Models.ViewModels
{
    public class ComplianceReportViewModel
    {
        [Display(Name = "Student or Internship")]
        public string? Search { get; set; }

        [Display(Name = "University")]
        public string? University { get; set; }

        [Display(Name = "Compliance Status")]
        public string? ComplianceStatus { get; set; }

        public bool IsAdminView { get; set; }

        public int TotalEnrollments { get; set; }

        public int UniversityApproved { get; set; }

        public int FullyCompliant { get; set; }

        public int NeedsAttention { get; set; }

        public int TotalRequiredHours { get; set; }

        public double TotalApprovedHours { get; set; }

        public double OverallComplianceRate { get; set; }

        public List<string> AvailableUniversities { get; set; }
            = new List<string>();

        public List<ComplianceStudentViewModel> Students { get; set; }
            = new List<ComplianceStudentViewModel>();
    }

    public class ComplianceStudentViewModel
    {
        public int TrainingEnrollmentId { get; set; }

        public string StudentName { get; set; }
            = string.Empty;

        public string University { get; set; }
            = string.Empty;

        public string Faculty { get; set; }
            = string.Empty;

        public string Specialization { get; set; }
            = string.Empty;

        public string InternshipTitle { get; set; }
            = string.Empty;

        public string CompanyName { get; set; }
            = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime ExpectedEndDate { get; set; }

        public int RequiredHours { get; set; }

        public double LoggedHours { get; set; }

        public double ApprovedHours { get; set; }

        public int WeeklyReportsCount { get; set; }

        public int ApprovedReportsCount { get; set; }

        public double HoursCompletionRate { get; set; }

        public bool IsUniversityApproved { get; set; }

        public bool IsTrainingCompleted { get; set; }

        public bool IsOverdue { get; set; }

        public string ComplianceStatus { get; set; }
            = string.Empty;

        public int ComplianceScore { get; set; }
    }
}