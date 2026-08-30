using InternshipPortal.Data;
using InternshipPortal.Models.Enums;
using InternshipPortal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Company")]
    public class TalentSearchController : Controller
    {
        private readonly ApplicationDbContext context;

        public TalentSearchController(
            ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            TalentSearchViewModel filters)
        {
            var portfolios = await context.StudentPortfolios
                .AsNoTracking()
                .Include(portfolio =>
                    portfolio.Student)
                .ThenInclude(student =>
                    student.Applications)
                .ThenInclude(application =>
                    application.Evaluation)
                .Include(portfolio =>
                    portfolio.Student)
                .ThenInclude(student =>
                    student.Applications)
                .ThenInclude(application =>
                    application.TrainingEnrollment)
                .ThenInclude(enrollment =>
                    enrollment!.HourEntries)
                .Include(portfolio =>
                    portfolio.Projects)
                .Where(portfolio =>
                    portfolio.IsPublic)
                .ToListAsync();

            var allCandidates = portfolios
                .Select(portfolio =>
                {
                    var completedApplications =
                        portfolio.Student.Applications
                            .Where(application =>
                                application.Status ==
                                    ApplicationStatus.Completed)
                            .ToList();

                    var ratings = completedApplications
                        .Where(application =>
                            application.Evaluation != null)
                        .Select(application =>
                            application.Evaluation!.Rating)
                        .ToList();

                    var verifiedHours = completedApplications
                        .Where(application =>
                            application.TrainingEnrollment != null)
                        .SelectMany(application =>
                            application.TrainingEnrollment!
                                .HourEntries)
                        .Where(entry =>
                            entry.Status ==
                                TrainingHourStatus.Approved)
                        .Sum(entry => entry.Hours);

                    var averageRating = ratings.Any()
                        ? ratings.Average()
                        : 0;

                    var skills = string.IsNullOrWhiteSpace(
                            portfolio.SkillsSummary)
                        ? new List<string>()
                        : portfolio.SkillsSummary
                            .Split(
                                ',',
                                StringSplitOptions.RemoveEmptyEntries)
                            .Select(skill => skill.Trim())
                            .Where(skill =>
                                !string.IsNullOrWhiteSpace(skill))
                            .Distinct(
                                StringComparer.OrdinalIgnoreCase)
                            .ToList();

                    var talentScore = CalculateTalentScore(
                        portfolio.Projects.Count,
                        completedApplications.Count,
                        Convert.ToInt32(verifiedHours),
                        averageRating,
                        skills.Count);

                    return new TalentCandidateViewModel
                    {
                        StudentId = portfolio.Student.Id,
                        FullName = portfolio.Student.FullName,
                        University =
                            portfolio.Student.University,
                        Faculty = portfolio.Student.Faculty,
                        Specialization =
                            portfolio.Student.Specialization,
                        GraduationYear =
                            portfolio.Student.GraduationYear,
                        Headline = portfolio.Headline,
                        SkillsSummary =
                            portfolio.SkillsSummary,
                        PortfolioSlug =
                            portfolio.PortfolioSlug,
                        ProjectsCount =
                            portfolio.Projects.Count,
                        CompletedTrainingsCount =
                            completedApplications.Count,
                        VerifiedTrainingHours =
                            Convert.ToInt32(verifiedHours),
                        EvaluationsCount = ratings.Count,
                        AverageRating = averageRating,
                        TalentScore = talentScore,
                        HasVerifiedExperience =
                            completedApplications.Any() ||
                            verifiedHours > 0 ||
                            ratings.Any(),
                        Skills = skills
                    };
                })
                .ToList();

            var candidates = allCandidates.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.Trim();

                candidates = candidates.Where(candidate =>
                    candidate.FullName.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase) ||
                    candidate.Headline.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase) ||
                    candidate.Specialization.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase) ||
                    candidate.University.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase) ||
                    candidate.Skills.Any(skill =>
                        skill.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(
                filters.Specialization))
            {
                candidates = candidates.Where(candidate =>
                    candidate.Specialization.Equals(
                        filters.Specialization,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(
                filters.University))
            {
                candidates = candidates.Where(candidate =>
                    candidate.University.Equals(
                        filters.University,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filters.Skill))
            {
                var requiredSkill = filters.Skill.Trim();

                candidates = candidates.Where(candidate =>
                    candidate.Skills.Any(skill =>
                        skill.Contains(
                            requiredSkill,
                            StringComparison.OrdinalIgnoreCase)));
            }

            if (filters.GraduationYear.HasValue)
            {
                candidates = candidates.Where(candidate =>
                    candidate.GraduationYear ==
                        filters.GraduationYear.Value);
            }

            if (filters.MinimumRating.HasValue)
            {
                candidates = candidates.Where(candidate =>
                    candidate.AverageRating >=
                        filters.MinimumRating.Value);
            }

            if (filters.MinimumVerifiedHours.HasValue)
            {
                candidates = candidates.Where(candidate =>
                    candidate.VerifiedTrainingHours >=
                        filters.MinimumVerifiedHours.Value);
            }

            candidates = filters.SortBy?.ToLower() switch
            {
                "rating" => candidates
                    .OrderByDescending(candidate =>
                        candidate.AverageRating)
                    .ThenByDescending(candidate =>
                        candidate.TalentScore),

                "hours" => candidates
                    .OrderByDescending(candidate =>
                        candidate.VerifiedTrainingHours)
                    .ThenByDescending(candidate =>
                        candidate.TalentScore),

                "projects" => candidates
                    .OrderByDescending(candidate =>
                        candidate.ProjectsCount)
                    .ThenByDescending(candidate =>
                        candidate.TalentScore),

                "graduation" => candidates
                    .OrderBy(candidate =>
                        candidate.GraduationYear)
                    .ThenByDescending(candidate =>
                        candidate.TalentScore),

                "name" => candidates
                    .OrderBy(candidate =>
                        candidate.FullName),

                _ => candidates
                    .OrderByDescending(candidate =>
                        candidate.TalentScore)
                    .ThenByDescending(candidate =>
                        candidate.AverageRating)
            };

            filters.Candidates = candidates.ToList();

            filters.AvailableSpecializations =
                allCandidates
                    .Select(candidate =>
                        candidate.Specialization)
                    .Where(value =>
                        !string.IsNullOrWhiteSpace(value))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .ToList();

            filters.AvailableUniversities =
                allCandidates
                    .Select(candidate =>
                        candidate.University)
                    .Where(value =>
                        !string.IsNullOrWhiteSpace(value))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .ToList();

            filters.TotalPublicProfiles =
                allCandidates.Count;

            filters.VerifiedCandidates =
                allCandidates.Count(candidate =>
                    candidate.HasVerifiedExperience);

            filters.TotalProjects =
                allCandidates.Sum(candidate =>
                    candidate.ProjectsCount);

            var ratedCandidates = allCandidates
                .Where(candidate =>
                    candidate.EvaluationsCount > 0)
                .ToList();

            filters.PlatformAverageRating =
                ratedCandidates.Any()
                    ? ratedCandidates.Average(candidate =>
                        candidate.AverageRating)
                    : 0;

            return View(filters);
        }

        private static int CalculateTalentScore(
            int projectsCount,
            int completedTrainings,
            int verifiedHours,
            double averageRating,
            int skillsCount)
        {
            var projectScore =
                Math.Min(projectsCount * 8, 24);

            var trainingScore =
                Math.Min(completedTrainings * 12, 24);

            var hoursScore =
                Math.Min(verifiedHours / 10, 20);

            var ratingScore =
                Convert.ToInt32(
                    averageRating / 5 * 22);

            var skillsScore =
                Math.Min(skillsCount * 2, 10);

            var totalScore =
                projectScore +
                trainingScore +
                hoursScore +
                ratingScore +
                skillsScore;

            return Math.Min(totalScore, 100);
        }
    }
}