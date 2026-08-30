namespace InternshipPortal.Models.ViewModels
{
    public class InternshipMatchViewModel
    {
        public Internship Internship { get; set; } = null!;

        public int MatchPercentage { get; set; }

        public string MatchLevel { get; set; } = string.Empty;

        public List<string> MatchedSkills { get; set; }
            = new List<string>();

        public List<string> MissingSkills { get; set; }
            = new List<string>();

        public List<string> MatchReasons { get; set; }
            = new List<string>();

        public bool HasApplied { get; set; }

        public bool IsSaved { get; set; }
    }
}