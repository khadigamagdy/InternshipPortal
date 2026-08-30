using InternshipPortal.Data;
using InternshipPortal.Models;
using InternshipPortal.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "UniversitySupervisor")]
    public class TrainingApprovalController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public TrainingApprovalController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var supervisorUserId =
                userManager.GetUserId(User);

            var enrollments =
                await context.TrainingEnrollments
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
                    .Include(enrollment =>
                        enrollment.HourEntries)
                    .Include(enrollment =>
                        enrollment.WeeklyReports)
                    .Where(enrollment =>
                        enrollment.Status ==
                            TrainingStatus
                                .PendingUniversityApproval ||
                        enrollment
                            .UniversitySupervisorUserId ==
                                supervisorUserId)
                    .OrderBy(enrollment =>
                        enrollment.Status ==
                            TrainingStatus
                                .PendingUniversityApproval
                                ? 0
                                : 1)
                    .ThenByDescending(enrollment =>
                        enrollment.CreatedAt)
                    .ToListAsync();

            return View(enrollments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var supervisorUserId =
                userManager.GetUserId(User);

            var enrollment =
                await GetEnrollmentAsync(id);

            if (enrollment == null)
            {
                return NotFound();
            }

            if (enrollment.Status !=
                TrainingStatus.PendingUniversityApproval)
            {
                TempData["ErrorMessage"] =
                    "Only pending training enrollments can be approved.";

                return RedirectToAction(nameof(Index));
            }

            enrollment.Status =
                TrainingStatus.Active;

            enrollment.UniversitySupervisorUserId =
                supervisorUserId;

            enrollment.UniversityApprovedAt =
                DateTime.Now;

            var studentUserId =
                enrollment
                    .InternshipApplication
                    .Student
                    .UserId;

            var companyUserId =
                enrollment
                    .InternshipApplication
                    .Internship
                    .Company
                    .UserId;

            var internshipTitle =
                enrollment
                    .InternshipApplication
                    .Internship
                    .Title;

            context.Notifications.Add(
                new Notification
                {
                    UserId = studentUserId,

                    Title = "Training Approved",

                    Message =
                        $"Your training for " +
                        $"'{internshipTitle}' has been approved " +
                        $"by the university supervisor. " +
                        $"You can now start recording your hours.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            context.Notifications.Add(
                new Notification
                {
                    UserId = companyUserId,

                    Title = "Training Enrollment Approved",

                    Message =
                        $"The university approved " +
                        $"{enrollment.InternshipApplication.Student.FullName}'s " +
                        $"training enrollment for " +
                        $"'{internshipTitle}'.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Training enrollment approved and activated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var supervisorUserId =
                userManager.GetUserId(User);

            var enrollment =
                await GetEnrollmentAsync(id);

            if (enrollment == null)
            {
                return NotFound();
            }

            if (enrollment.Status !=
                TrainingStatus.PendingUniversityApproval)
            {
                TempData["ErrorMessage"] =
                    "Only pending training enrollments can be rejected.";

                return RedirectToAction(nameof(Index));
            }

            enrollment.Status =
                TrainingStatus.Cancelled;

            enrollment.UniversitySupervisorUserId =
                supervisorUserId;

            var internshipTitle =
                enrollment
                    .InternshipApplication
                    .Internship
                    .Title;

            context.Notifications.Add(
                new Notification
                {
                    UserId =
                        enrollment
                            .InternshipApplication
                            .Student
                            .UserId,

                    Title = "Training Enrollment Rejected",

                    Message =
                        $"Your training enrollment for " +
                        $"'{internshipTitle}' was not approved " +
                        $"by the university supervisor.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            context.Notifications.Add(
                new Notification
                {
                    UserId =
                        enrollment
                            .InternshipApplication
                            .Internship
                            .Company
                            .UserId,

                    Title = "Training Enrollment Rejected",

                    Message =
                        $"The university did not approve " +
                        $"{enrollment.InternshipApplication.Student.FullName}'s " +
                        $"training enrollment for " +
                        $"'{internshipTitle}'.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Training enrollment rejected.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<TrainingEnrollment?>
            GetEnrollmentAsync(int id)
        {
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
                    enrollment.Id == id);
        }
    }
}