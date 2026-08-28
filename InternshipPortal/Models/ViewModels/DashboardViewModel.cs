namespace InternshipPortal.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string AccountType { get; set; }
            = string.Empty;

        public string DisplayName { get; set; }
            = string.Empty;

        public bool ProfileCompleted { get; set; }

        public int? StudentId { get; set; }

        public int? CompanyId { get; set; }

        public int TotalStudents { get; set; }

        public int TotalCompanies { get; set; }

        public int TotalInternships { get; set; }

        public int ApprovedInternships { get; set; }

        public int TotalApplications { get; set; }

        public int PendingApplications { get; set; }

        public int AcceptedApplications { get; set; }

        public int StudentApplications { get; set; }

        public int StudentPendingApplications { get; set; }

        public int StudentAcceptedApplications { get; set; }

        public int StudentSavedInternships { get; set; }

        public int StudentUpcomingInterviews { get; set; }

        public int CompanyInternships { get; set; }

        public int CompanyApprovedInternships { get; set; }

        public int CompanyPendingInternships { get; set; }

        public int CompanyApplicants { get; set; }

        public int CompanyUpcomingInterviews { get; set; }

        public List<Internship> RecentInternships { get; set; }
            = new List<Internship>();

        public List<InternshipApplication> RecentApplications { get; set; }
            = new List<InternshipApplication>();

        public List<Interview> UpcomingInterviews { get; set; }
            = new List<Interview>();

        public List<SavedInternship> RecentSavedInternships { get; set; }
            = new List<SavedInternship>();
    }
}