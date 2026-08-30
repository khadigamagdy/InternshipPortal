using InternshipPortal.Data;
using InternshipPortal.Models;
using InternshipPortal.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Company")]
    public class TrainingHoursReviewController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public TrainingHoursReviewController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var companyUserId =
                userManager.GetUserId(User);

            var entries =
                await context.TrainingHourEntries
                    .Include(entry =>
                        entry.TrainingEnrollment)
                    .ThenInclude(enrollment =>
                        enrollment.InternshipApplication)
                    .ThenInclude(application =>
                        application.Student)
                    .Include(entry =>
                        entry.TrainingEnrollment)
                    .ThenInclude(enrollment =>
                        enrollment.InternshipApplication)
                    .ThenInclude(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                    .Where(entry =>
                        entry
                            .TrainingEnrollment
                            .InternshipApplication
                            .Internship
                            .Company
                            .UserId == companyUserId)
                    .OrderBy(entry =>
                        entry.Status ==
                            TrainingHourStatus.Pending
                            ? 0
                            : 1)
                    .ThenByDescending(entry =>
                        entry.CreatedAt)
                    .ToListAsync();

            return View(entries);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(
            int id,
            string? companyComment)
        {
            var companyUserId =
                userManager.GetUserId(User);

            var entry =
                await GetCompanyEntryAsync(
                    id,
                    companyUserId);

            if (entry == null)
            {
                return NotFound();
            }

            if (entry.Status !=
                TrainingHourStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "Only pending training hours can be approved.";

                return RedirectToAction(nameof(Index));
            }

            entry.Status =
                TrainingHourStatus.Approved;

            entry.CompanyComment =
                string.IsNullOrWhiteSpace(companyComment)
                    ? "Training hours verified and approved."
                    : companyComment.Trim();

            entry.ReviewedAt =
                DateTime.Now;

            var student =
                entry
                    .TrainingEnrollment
                    .InternshipApplication
                    .Student;

            var internship =
                entry
                    .TrainingEnrollment
                    .InternshipApplication
                    .Internship;

            context.Notifications.Add(
                new Notification
                {
                    UserId = student.UserId,

                    Title = "Training Hours Approved",

                    Message =
                        $"{entry.Hours:0.0} training hours " +
                        $"for {entry.TrainingDate:dd MMM yyyy} " +
                        $"were approved by {internship.Company.Name}.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Training hours approved successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(
            int id,
            string? companyComment)
        {
            var companyUserId =
                userManager.GetUserId(User);

            var entry =
                await GetCompanyEntryAsync(
                    id,
                    companyUserId);

            if (entry == null)
            {
                return NotFound();
            }

            if (entry.Status !=
                TrainingHourStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "Only pending training hours can be rejected.";

                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(companyComment))
            {
                TempData["ErrorMessage"] =
                    "Please provide a rejection reason.";

                return RedirectToAction(nameof(Index));
            }

            entry.Status =
                TrainingHourStatus.Rejected;

            entry.CompanyComment =
                companyComment.Trim();

            entry.ReviewedAt =
                DateTime.Now;

            var student =
                entry
                    .TrainingEnrollment
                    .InternshipApplication
                    .Student;

            var internship =
                entry
                    .TrainingEnrollment
                    .InternshipApplication
                    .Internship;

            context.Notifications.Add(
                new Notification
                {
                    UserId = student.UserId,

                    Title = "Training Hours Returned",

                    Message =
                        $"{entry.Hours:0.0} training hours " +
                        $"for {entry.TrainingDate:dd MMM yyyy} " +
                        $"were returned for correction. " +
                        $"Reason: {entry.CompanyComment}",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Training hours rejected and returned to the student.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<TrainingHourEntry?>
            GetCompanyEntryAsync(
                int entryId,
                string? companyUserId)
        {
            return await context.TrainingHourEntries
                .Include(entry =>
                    entry.TrainingEnrollment)
                .ThenInclude(enrollment =>
                    enrollment.InternshipApplication)
                .ThenInclude(application =>
                    application.Student)
                .Include(entry =>
                    entry.TrainingEnrollment)
                .ThenInclude(enrollment =>
                    enrollment.InternshipApplication)
                .ThenInclude(application =>
                    application.Internship)
                .ThenInclude(internship =>
                    internship.Company)
                .FirstOrDefaultAsync(entry =>
                    entry.Id == entryId &&
                    entry
                        .TrainingEnrollment
                        .InternshipApplication
                        .Internship
                        .Company
                        .UserId == companyUserId);
        }
    }
}