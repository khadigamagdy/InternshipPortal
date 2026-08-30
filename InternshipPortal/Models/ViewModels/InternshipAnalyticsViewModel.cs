namespace InternshipPortal.Models.ViewModels
{
    public class InternshipAnalyticsViewModel
    {
        public bool IsAdminView { get; set; }

        public string DashboardOwnerName { get; set; }
            = string.Empty;

        public int TotalInternships { get; set; }

        public int ActiveInternships { get; set; }

        public int TotalApplications { get; set; }

        public int PendingApplications { get; set; }

        public int AcceptedApplications { get; set; }

        public int RejectedApplications { get; set; }

        public int CompletedTrainings { get; set; }

        public double AcceptanceRate { get; set; }

        public double RejectionRate { get; set; }

        public double CompletionRate { get; set; }

        public double AverageRating { get; set; }

        public double AverageReviewHours { get; set; }

        public string BestInternshipTitle { get; set; }
            = "No data yet";

        public double BestInternshipScore { get; set; }

        public List<InternshipPerformanceViewModel>
            InternshipPerformance
        { get; set; }
                = new List<InternshipPerformanceViewModel>();

        public List<MonthlyApplicationTrendViewModel>
            MonthlyTrends
        { get; set; }
                = new List<MonthlyApplicationTrendViewModel>();
    }

    public class InternshipPerformanceViewModel
    {
        public int InternshipId { get; set; }

        public string InternshipTitle { get; set; }
            = string.Empty;

        public string CompanyName { get; set; }
            = string.Empty;

        public bool IsActive { get; set; }

        public int TotalApplications { get; set; }

        public int AcceptedApplications { get; set; }

        public int RejectedApplications { get; set; }

        public int CompletedTrainings { get; set; }

        public int RemainingPositions { get; set; }

        public double AcceptanceRate { get; set; }

        public double CompletionRate { get; set; }

        public double AverageRating { get; set; }

        public double AverageReviewHours { get; set; }

        public int QualityScore { get; set; }
    }

    public class MonthlyApplicationTrendViewModel
    {
        public string MonthName { get; set; }
            = string.Empty;

        public int Applications { get; set; }

        public int Accepted { get; set; }

        public int Completed { get; set; }
    }
}