using InternshipPortal.Data;
using InternshipPortal.Models;
using InternshipPortal.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Student")]
    public class SkillRoadmapController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public SkillRoadmapController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                TempData["Error"] =
                    "Please complete your student profile first.";

                return RedirectToAction(
                    "Profile",
                    "Student");
            }

            var plans = await context.SkillDevelopmentPlans
                .Include(plan => plan.Internship)
                .ThenInclude(internship => internship.Company)
                .Include(plan => plan.Items)
                .Where(plan => plan.StudentId == student.Id)
                .OrderBy(plan => plan.IsCompleted)
                .ThenByDescending(plan => plan.CreatedAt)
                .ToListAsync();

            return View(plans);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(
            int internshipId)
        {
            var student = await GetCurrentStudentAsync(
                includePreference: true);

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

            var internship = await context.Internships
                .Include(internship => internship.Company)
                .FirstOrDefaultAsync(internship =>
                    internship.Id == internshipId &&
                    internship.IsApproved &&
                    internship.IsActive);

            if (internship == null)
            {
                return NotFound();
            }

            var existingPlan =
                await context.SkillDevelopmentPlans
                    .FirstOrDefaultAsync(plan =>
                        plan.StudentId == student.Id &&
                        plan.InternshipId == internshipId);

            if (existingPlan != null)
            {
                TempData["SuccessMessage"] =
                    "Your development roadmap is already available.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = existingPlan.Id });
            }

            var studentSkills = SplitSkills(
                student.Preference.Skills);

            var requiredSkills = SplitSkills(
                internship.RequiredSkills);

            var missingSkills = requiredSkills
                .Where(requiredSkill =>
                    !studentSkills.Any(studentSkill =>
                        IsSimilarSkill(
                            studentSkill,
                            requiredSkill)))
                .ToList();

            if (!missingSkills.Any())
            {
                TempData["SuccessMessage"] =
                    "Great! You already match all listed skills for this internship.";

                return RedirectToAction(
                    "Index",
                    "Matching");
            }

            var plan = new SkillDevelopmentPlan
            {
                Title =
                    $"{internship.Title} Preparation Roadmap",

                StudentId = student.Id,
                InternshipId = internship.Id,
                CreatedAt = DateTime.Now,
                TargetCompletionDate =
                    DateTime.Today.AddDays(
                        missingSkills.Count * 7),
                IsCompleted = false
            };

            var displayOrder = 1;

            foreach (var skill in missingSkills)
            {
                plan.Items.Add(
                    new SkillPlanItem
                    {
                        SkillName = skill,

                        LearningGoal =
                            $"Learn the essential concepts of {skill} " +
                            $"and complete a practical project using it.",

                        LearningResourceUrl =
                            CreateLearningResourceUrl(skill),

                        Status =
                            SkillProgressStatus.NotStarted,

                        ProgressPercentage = 0,
                        DisplayOrder = displayOrder
                    });

                displayOrder++;
            }

            context.SkillDevelopmentPlans.Add(plan);

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"A roadmap with {missingSkills.Count} skills was created successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = plan.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return NotFound();
            }

            var plan = await context.SkillDevelopmentPlans
                .Include(plan => plan.Internship)
                .ThenInclude(internship => internship.Company)
                .Include(plan => plan.Items
                    .OrderBy(item => item.DisplayOrder))
                .FirstOrDefaultAsync(plan =>
                    plan.Id == id &&
                    plan.StudentId == student.Id);

            if (plan == null)
            {
                return NotFound();
            }

            ViewBag.CompletedItems =
                plan.Items.Count(item =>
                    item.Status ==
                        SkillProgressStatus.Completed);

            ViewBag.TotalItems = plan.Items.Count;

            ViewBag.OverallProgress =
                CalculateOverallProgress(plan.Items);

            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateItem(
            int itemId,
            SkillProgressStatus status,
            int progressPercentage)
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return NotFound();
            }

            var item = await context.SkillPlanItems
                .Include(item => item.SkillDevelopmentPlan)
                .ThenInclude(plan => plan.Items)
                .FirstOrDefaultAsync(item =>
                    item.Id == itemId &&
                    item.SkillDevelopmentPlan.StudentId ==
                        student.Id);

            if (item == null)
            {
                return NotFound();
            }

            progressPercentage =
                Math.Clamp(progressPercentage, 0, 100);

            item.Status = status;
            item.ProgressPercentage = progressPercentage;

            if (status == SkillProgressStatus.NotStarted)
            {
                item.ProgressPercentage = 0;
                item.StartedAt = null;
                item.CompletedAt = null;
            }
            else if (status == SkillProgressStatus.InProgress)
            {
                if (item.ProgressPercentage == 0 ||
                    item.ProgressPercentage == 100)
                {
                    item.ProgressPercentage = 50;
                }

                item.StartedAt ??= DateTime.Now;
                item.CompletedAt = null;
            }
            else if (status == SkillProgressStatus.Completed)
            {
                item.ProgressPercentage = 100;
                item.StartedAt ??= DateTime.Now;
                item.CompletedAt = DateTime.Now;
            }

            var plan = item.SkillDevelopmentPlan;

            var allItemsCompleted = plan.Items.All(planItem =>
                planItem.Id == item.Id
                    ? item.Status ==
                        SkillProgressStatus.Completed
                    : planItem.Status ==
                        SkillProgressStatus.Completed);

            plan.IsCompleted = allItemsCompleted;

            if (allItemsCompleted)
            {
                plan.CompletedAt = DateTime.Now;
            }
            else
            {
                plan.CompletedAt = null;
            }

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Skill progress updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = plan.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return NotFound();
            }

            var plan = await context.SkillDevelopmentPlans
                .FirstOrDefaultAsync(plan =>
                    plan.Id == id &&
                    plan.StudentId == student.Id);

            if (plan == null)
            {
                return NotFound();
            }

            context.SkillDevelopmentPlans.Remove(plan);

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Development roadmap deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<Student?> GetCurrentStudentAsync(
            bool includePreference = false)
        {
            var userId = userManager.GetUserId(User);

            var query = context.Students.AsQueryable();

            if (includePreference)
            {
                query = query.Include(student =>
                    student.Preference);
            }

            return await query.FirstOrDefaultAsync(student =>
                student.UserId == userId);
        }

        private int CalculateOverallProgress(
            ICollection<SkillPlanItem> items)
        {
            if (!items.Any())
            {
                return 0;
            }

            return (int)Math.Round(
                items.Average(item =>
                    item.ProgressPercentage));
        }

        private List<string> SplitSkills(string? value)
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
                .Select(skill => skill.Trim())
                .Where(skill => skill.Length > 1)
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

        private string CreateLearningResourceUrl(
            string skill)
        {
            var searchText =
                Uri.EscapeDataString(
                    $"{skill} learning path");

            return
                $"https://learn.microsoft.com/en-us/search/" +
                $"?terms={searchText}";
        }
    }
}