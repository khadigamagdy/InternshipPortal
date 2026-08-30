using InternshipPortal.Data;
using InternshipPortal.Models;
using InternshipPortal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Student")]
    public class MatchingController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public MatchingController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = userManager.GetUserId(User);

            var student = await context.Students
                .Include(student => student.Preference)
                .FirstOrDefaultAsync(student =>
                    student.UserId == userId);

            if (student == null)
            {
                TempData["Error"] =
                    "Please complete your student profile first.";

                return RedirectToAction(
                    "Profile",
                    "Student");
            }

            if (student.Preference == null ||
                string.IsNullOrWhiteSpace(
                    student.Preference.Skills))
            {
                TempData["Error"] =
                    "Please add your career preferences first.";

                return RedirectToAction(
                    "Index",
                    "StudentPreference");
            }

            var internships = await context.Internships
                .Include(internship => internship.Company)
                .Where(internship =>
                    internship.IsApproved &&
                    internship.IsActive &&
                    internship.ApplicationDeadline >= DateTime.Today &&
                    internship.AvailablePositions > 0)
                .ToListAsync();

            var appliedInternshipIds =
                await context.InternshipApplications
                    .Where(application =>
                        application.StudentId == student.Id)
                    .Select(application =>
                        application.InternshipId)
                    .ToListAsync();

            var savedInternshipIds =
                await context.SavedInternships
                    .Where(savedInternship =>
                        savedInternship.StudentId == student.Id)
                    .Select(savedInternship =>
                        savedInternship.InternshipId)
                    .ToListAsync();

            var matches = new List<InternshipMatchViewModel>();

            foreach (var internship in internships)
            {
                var match = CalculateMatch(
                    student,
                    student.Preference,
                    internship);

                match.HasApplied =
                    appliedInternshipIds.Contains(internship.Id);

                match.IsSaved =
                    savedInternshipIds.Contains(internship.Id);

                matches.Add(match);
            }

            matches = matches
                .OrderByDescending(match =>
                    match.MatchPercentage)
                .ThenBy(internship =>
                    internship.Internship.ApplicationDeadline)
                .ToList();

            ViewBag.StudentName = student.FullName;
            ViewBag.TotalMatches = matches.Count;
            ViewBag.StrongMatches = matches.Count(match =>
                match.MatchPercentage >= 75);

            return View(matches);
        }

        private InternshipMatchViewModel CalculateMatch(
            Student student,
            StudentPreference preference,
            Internship internship)
        {
            var score = 0;

            var matchedSkills = new List<string>();
            var missingSkills = new List<string>();
            var reasons = new List<string>();

            var studentSkills = SplitWords(preference.Skills);

            var requiredSkills =
                SplitWords(internship.RequiredSkills);

            foreach (var requiredSkill in requiredSkills)
            {
                var hasSkill = studentSkills.Any(studentSkill =>
                    IsSimilarSkill(studentSkill, requiredSkill));

                if (hasSkill)
                {
                    matchedSkills.Add(requiredSkill);
                }
                else
                {
                    missingSkills.Add(requiredSkill);
                }
            }

            if (requiredSkills.Count > 0)
            {
                var skillScore = (int)Math.Round(
                    matchedSkills.Count * 50.0 /
                    requiredSkills.Count);

                score += skillScore;

                if (matchedSkills.Any())
                {
                    reasons.Add(
                        $"{matchedSkills.Count} required skills matched.");
                }
            }

            var careerText =
                $"{preference.CareerInterests} " +
                $"{student.Specialization}";

            var internshipText =
                $"{internship.Title} " +
                $"{internship.Description} " +
                $"{internship.RequiredSkills}";

            var careerWords = SplitWords(careerText);

            var careerMatched = careerWords.Any(word =>
                internshipText.Contains(
                    word,
                    StringComparison.OrdinalIgnoreCase));

            if (careerMatched)
            {
                score += 15;

                reasons.Add(
                    "Matches your specialization or career interests.");
            }

            if (!string.IsNullOrWhiteSpace(
                    preference.PreferredLocation) &&
                internship.Location.Contains(
                    preference.PreferredLocation,
                    StringComparison.OrdinalIgnoreCase))
            {
                score += 10;

                reasons.Add("Matches your preferred location.");
            }

            if (preference.PreferredWorkMode.HasValue &&
                internship.WorkMode ==
                    preference.PreferredWorkMode.Value)
            {
                score += 10;

                reasons.Add("Matches your preferred work mode.");
            }

            if (internship.IsPaid)
            {
                if (!preference.MinimumSalary.HasValue ||
                    internship.Salary >=
                        preference.MinimumSalary.Value)
                {
                    score += 10;

                    reasons.Add(
                        "Salary matches your expectations.");
                }
                else
                {
                    score += 4;
                }
            }
            else if (preference.AcceptUnpaidInternships)
            {
                score += 10;

                reasons.Add(
                    "Matches your unpaid training preference.");
            }

            var remainingDays =
                (internship.ApplicationDeadline -
                    DateTime.Today).Days;

            if (remainingDays >= 7)
            {
                score += 5;

                reasons.Add(
                    "Application deadline is still available.");
            }

            if (score > 100)
            {
                score = 100;
            }

            var matchLevel = score switch
            {
                >= 85 => "Excellent Match",
                >= 70 => "Strong Match",
                >= 50 => "Good Match",
                >= 30 => "Possible Match",
                _ => "Low Match"
            };

            return new InternshipMatchViewModel
            {
                Internship = internship,
                MatchPercentage = score,
                MatchLevel = matchLevel,
                MatchedSkills = matchedSkills
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                MissingSkills = missingSkills
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                MatchReasons = reasons,
                HasApplied = false,
                IsSaved = false
            };
        }

        private List<string> SplitWords(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new List<string>();
            }

            var separators = new[]
            {
                ',',
                ';',
                '|',
                '\n',
                '\r'
            };

            return value
                .Split(
                    separators,
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word.Trim())
                .Where(word => word.Length > 1)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private bool IsSimilarSkill(
            string studentSkill,
            string requiredSkill)
        {
            return studentSkill.Equals(
                       requiredSkill,
                       StringComparison.OrdinalIgnoreCase) ||
                   studentSkill.Contains(
                       requiredSkill,
                       StringComparison.OrdinalIgnoreCase) ||
                   requiredSkill.Contains(
                       studentSkill,
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}