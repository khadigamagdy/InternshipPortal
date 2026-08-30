namespace InternshipPortal.Models.ViewModels
{
    public class StudentPortfolioDetailsViewModel
    {
        public Student Student { get; set; } = null!;

        public StudentPortfolio Portfolio { get; set; } = null!;

        public List<PortfolioProject> Projects { get; set; }
            = new List<PortfolioProject>();

        public List<InternshipApplication> CompletedTrainings { get; set; }
            = new List<InternshipApplication>();

        public int CompletedTrainingsCount { get; set; }

        public int ApprovedTrainingHours { get; set; }

        public int ProjectsCount { get; set; }

        public int EvaluationsCount { get; set; }

        public double AverageRating { get; set; }
    }
}