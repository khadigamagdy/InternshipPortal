using InternshipPortal.Data;
using InternshipPortal.Models;
using InternshipPortal.Models.Enums;
using InternshipPortal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize]
    public class WeeklyReportController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public WeeklyReportController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId =
                userManager.GetUserId(User);

            var reports =
                await context.WeeklyReports
                    .Include(report =>
                        report.TrainingEnrollment)
                    .ThenInclude(enrollment =>
                        enrollment.InternshipApplication)
                    .ThenInclude(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                    .Where(report =>
                        report
                            .TrainingEnrollment
                            .InternshipApplication
                            .Student
                            .UserId == userId)
                    .OrderByDescending(report =>
                        report.CreatedAt)
                    .ToListAsync();

            return View(reports);
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Create(
            int enrollmentId)
        {
            var enrollment =
                await GetStudentEnrollmentAsync(
                    enrollmentId);

            if (enrollment == null)
            {
                return NotFound();
            }

            if (enrollment.Status !=
                TrainingStatus.Active)
            {
                TempData["ErrorMessage"] =
                    "Weekly reports can only be created for active training.";

                return RedirectToAction(
                    "Index",
                    "TrainingLogbook");
            }

            var lastWeekNumber =
                await context.WeeklyReports
                    .Where(report =>
                        report.TrainingEnrollmentId ==
                            enrollmentId)
                    .MaxAsync(report =>
                        (int?)report.WeekNumber) ?? 0;

            var nextWeekNumber =
                lastWeekNumber + 1;

            if (nextWeekNumber > 52)
            {
                TempData["ErrorMessage"] =
                    "The maximum number of weekly reports has been reached.";

                return RedirectToAction(nameof(Index));
            }

            var weekStartDate =
                enrollment.StartDate.Date
                    .AddDays((nextWeekNumber - 1) * 7);

            var weekEndDate =
                weekStartDate.AddDays(6);

            if (weekEndDate >
                enrollment.ExpectedEndDate.Date)
            {
                weekEndDate =
                    enrollment.ExpectedEndDate.Date;
            }

            var model = new WeeklyReportViewModel
            {
                TrainingEnrollmentId =
                    enrollment.Id,

                InternshipTitle =
                    enrollment
                        .InternshipApplication
                        .Internship
                        .Title,

                CompanyName =
                    enrollment
                        .InternshipApplication
                        .Internship
                        .Company
                        .Name,

                WeekNumber =
                    nextWeekNumber,

                WeekStartDate =
                    weekStartDate,

                WeekEndDate =
                    weekEndDate
            };

            return View(model);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            WeeklyReportViewModel model,
            bool submitReport)
        {
            var enrollment =
                await GetStudentEnrollmentAsync(
                    model.TrainingEnrollmentId);

            if (enrollment == null)
            {
                return NotFound();
            }

            model.InternshipTitle =
                enrollment
                    .InternshipApplication
                    .Internship
                    .Title;

            model.CompanyName =
                enrollment
                    .InternshipApplication
                    .Internship
                    .Company
                    .Name;

            if (enrollment.Status !=
                TrainingStatus.Active)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This training is not active.");
            }

            if (model.WeekEndDate.Date <
                model.WeekStartDate.Date)
            {
                ModelState.AddModelError(
                    nameof(model.WeekEndDate),
                    "Week end date cannot be before the start date.");
            }

            if (model.WeekStartDate.Date <
                    enrollment.StartDate.Date ||
                model.WeekEndDate.Date >
                    enrollment.ExpectedEndDate.Date)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The report dates must be within the training period.");
            }

            var reportAlreadyExists =
                await context.WeeklyReports
                    .AnyAsync(report =>
                        report.TrainingEnrollmentId ==
                            enrollment.Id &&
                        report.WeekNumber ==
                            model.WeekNumber);

            if (reportAlreadyExists)
            {
                ModelState.AddModelError(
                    nameof(model.WeekNumber),
                    "A report already exists for this week.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var report = new WeeklyReport
            {
                TrainingEnrollmentId =
                    enrollment.Id,

                WeekNumber =
                    model.WeekNumber,

                WeekStartDate =
                    model.WeekStartDate.Date,

                WeekEndDate =
                    model.WeekEndDate.Date,

                TasksCompleted =
                    model.TasksCompleted.Trim(),

                SkillsLearned =
                    model.SkillsLearned.Trim(),

                Challenges =
                    string.IsNullOrWhiteSpace(
                        model.Challenges)
                        ? null
                        : model.Challenges.Trim(),

                NextWeekPlan =
                    string.IsNullOrWhiteSpace(
                        model.NextWeekPlan)
                        ? null
                        : model.NextWeekPlan.Trim(),

                Status =
                    submitReport
                        ? WeeklyReportStatus.Submitted
                        : WeeklyReportStatus.Draft,

                CreatedAt =
                    DateTime.Now,

                SubmittedAt =
                    submitReport
                        ? DateTime.Now
                        : null
            };

            context.WeeklyReports.Add(report);

            if (submitReport)
            {
                context.Notifications.Add(
                    new Notification
                    {
                        UserId =
                            enrollment
                                .InternshipApplication
                                .Internship
                                .Company
                                .UserId,

                        Title =
                            "Weekly Training Report Submitted",

                        Message =
                            $"{enrollment.InternshipApplication.Student.FullName} " +
                            $"submitted week {model.WeekNumber} " +
                            $"training report for " +
                            $"'{model.InternshipTitle}'.",

                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
            }

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                submitReport
                    ? "Weekly report submitted to the company successfully."
                    : "Weekly report saved as draft.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var userId =
                userManager.GetUserId(User);

            var report =
                await context.WeeklyReports
                    .Include(item =>
                        item.TrainingEnrollment)
                    .ThenInclude(enrollment =>
                        enrollment.InternshipApplication)
                    .ThenInclude(application =>
                        application.Student)
                    .Include(item =>
                        item.TrainingEnrollment)
                    .ThenInclude(enrollment =>
                        enrollment.InternshipApplication)
                    .ThenInclude(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                    .FirstOrDefaultAsync(item =>
                        item.Id == id &&
                        item
                            .TrainingEnrollment
                            .InternshipApplication
                            .Student
                            .UserId == userId);

            if (report == null)
            {
                return NotFound();
            }

            if (report.Status !=
                    WeeklyReportStatus.Draft &&
                report.Status !=
                    WeeklyReportStatus.SupervisorReturned &&
                report.Status !=
                    WeeklyReportStatus.CompanyRejected)
            {
                TempData["ErrorMessage"] =
                    "This report cannot be submitted.";

                return RedirectToAction(nameof(Index));
            }

            report.Status =
                WeeklyReportStatus.Submitted;

            report.SubmittedAt =
                DateTime.Now;

            report.CompanyFeedback =
                null;

            report.SupervisorFeedback =
                null;

            context.Notifications.Add(
                new Notification
                {
                    UserId =
                        report
                            .TrainingEnrollment
                            .InternshipApplication
                            .Internship
                            .Company
                            .UserId,

                    Title =
                        "Weekly Training Report Submitted",

                    Message =
                        $"{report.TrainingEnrollment.InternshipApplication.Student.FullName} " +
                        $"submitted week {report.WeekNumber} " +
                        $"training report.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Weekly report submitted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<TrainingEnrollment?>
            GetStudentEnrollmentAsync(
                int enrollmentId)
        {
            var userId =
                userManager.GetUserId(User);

            return await context.TrainingEnrollments
                .Include(enrollment =>
                    enrollment.InternshipApplication)
                .ThenInclude(application =>
                    application.Student)
                .Include(enrollment =>
                    enrollment.InternshipApplication)
                .ThenInclude(application =>
                    application.Internship)
                .ThenInclude(internship =>
                    internship.Company)
                .FirstOrDefaultAsync(enrollment =>
                    enrollment.Id == enrollmentId &&
                    enrollment
                        .InternshipApplication
                        .Student
                        .UserId == userId);
        }
    }
}