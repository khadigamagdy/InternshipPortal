using InternshipPortal.Data;
using InternshipPortal.Models;
using InternshipPortal.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(
        Roles = "Company,UniversitySupervisor")]
    public class WeeklyReportReviewController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public WeeklyReportReviewController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId =
                userManager.GetUserId(User);

            var query =
                context.WeeklyReports
                    .Include(report =>
                        report.TrainingEnrollment)
                    .ThenInclude(enrollment =>
                        enrollment.InternshipApplication)
                    .ThenInclude(application =>
                        application.Student)
                    .Include(report =>
                        report.TrainingEnrollment)
                    .ThenInclude(enrollment =>
                        enrollment.InternshipApplication)
                    .ThenInclude(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                    .AsQueryable();

            if (User.IsInRole("Company"))
            {
                query = query.Where(report =>
                    report
                        .TrainingEnrollment
                        .InternshipApplication
                        .Internship
                        .Company
                        .UserId == userId &&
                    report.Status !=
                        WeeklyReportStatus.Draft);
            }
            else
            {
                query = query.Where(report =>
                    report
                        .TrainingEnrollment
                        .UniversitySupervisorUserId == userId &&
                    (
                        report.Status ==
                            WeeklyReportStatus.CompanyApproved ||
                        report.Status ==
                            WeeklyReportStatus.SupervisorApproved ||
                        report.Status ==
                            WeeklyReportStatus.SupervisorReturned
                    ));
            }

            var reports =
                await query
                    .OrderBy(report =>
                        report.Status ==
                            WeeklyReportStatus.Submitted ||
                        report.Status ==
                            WeeklyReportStatus.CompanyApproved
                            ? 0
                            : 1)
                    .ThenByDescending(report =>
                        report.CreatedAt)
                    .ToListAsync();

            return View(reports);
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompanyApprove(
            int id,
            string? feedback,
            int rating)
        {
            var companyUserId =
                userManager.GetUserId(User);

            var report =
                await GetCompanyReportAsync(
                    id,
                    companyUserId);

            if (report == null)
            {
                return NotFound();
            }

            if (report.Status !=
                WeeklyReportStatus.Submitted)
            {
                TempData["ErrorMessage"] =
                    "Only submitted reports can be approved.";

                return RedirectToAction(nameof(Index));
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] =
                    "Please select a rating between 1 and 5.";

                return RedirectToAction(nameof(Index));
            }

            report.Status =
                WeeklyReportStatus.CompanyApproved;

            report.CompanyFeedback =
                string.IsNullOrWhiteSpace(feedback)
                    ? "The weekly report was reviewed and approved."
                    : feedback.Trim();

            report.CompanyRating = rating;
            report.CompanyReviewedAt = DateTime.Now;

            var enrollment =
                report.TrainingEnrollment;

            var application =
                enrollment.InternshipApplication;

            var internship =
                application.Internship;

            if (!string.IsNullOrWhiteSpace(
                enrollment.UniversitySupervisorUserId))
            {
                context.Notifications.Add(
                    new Notification
                    {
                        UserId =
                            enrollment
                                .UniversitySupervisorUserId,

                        Title =
                            "Weekly Report Awaiting Approval",

                        Message =
                            $"{application.Student.FullName}'s " +
                            $"week {report.WeekNumber} report for " +
                            $"'{internship.Title}' was approved " +
                            $"by the company and requires your review.",

                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
            }

            context.Notifications.Add(
                new Notification
                {
                    UserId =
                        application.Student.UserId,

                    Title =
                        "Weekly Report Company Approved",

                    Message =
                        $"Your week {report.WeekNumber} report " +
                        $"was approved by {internship.Company.Name} " +
                        $"and sent to the university supervisor.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Weekly report approved and sent to the university supervisor.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompanyReject(
            int id,
            string? feedback)
        {
            var companyUserId =
                userManager.GetUserId(User);

            var report =
                await GetCompanyReportAsync(
                    id,
                    companyUserId);

            if (report == null)
            {
                return NotFound();
            }

            if (report.Status !=
                WeeklyReportStatus.Submitted)
            {
                TempData["ErrorMessage"] =
                    "Only submitted reports can be returned.";

                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(feedback))
            {
                TempData["ErrorMessage"] =
                    "Please write the reason for returning the report.";

                return RedirectToAction(nameof(Index));
            }

            report.Status =
                WeeklyReportStatus.CompanyRejected;

            report.CompanyFeedback =
                feedback.Trim();

            report.CompanyRating = null;
            report.CompanyReviewedAt = DateTime.Now;

            context.Notifications.Add(
                new Notification
                {
                    UserId =
                        report
                            .TrainingEnrollment
                            .InternshipApplication
                            .Student
                            .UserId,

                    Title =
                        "Weekly Report Returned",

                    Message =
                        $"Your week {report.WeekNumber} report " +
                        $"was returned by the company. " +
                        $"Reason: {report.CompanyFeedback}",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Weekly report returned to the student.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "UniversitySupervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupervisorApprove(
            int id,
            string? feedback,
            int rating)
        {
            var supervisorUserId =
                userManager.GetUserId(User);

            var report =
                await GetSupervisorReportAsync(
                    id,
                    supervisorUserId);

            if (report == null)
            {
                return NotFound();
            }

            if (report.Status !=
                WeeklyReportStatus.CompanyApproved)
            {
                TempData["ErrorMessage"] =
                    "The company must approve the report first.";

                return RedirectToAction(nameof(Index));
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] =
                    "Please select a rating between 1 and 5.";

                return RedirectToAction(nameof(Index));
            }

            var currentDate = DateTime.Now;

            report.Status =
                WeeklyReportStatus.SupervisorApproved;

            report.SupervisorFeedback =
                string.IsNullOrWhiteSpace(feedback)
                    ? "The weekly report was academically approved."
                    : feedback.Trim();

            report.SupervisorRating = rating;
            report.SupervisorReviewedAt = currentDate;

            var enrollment =
                report.TrainingEnrollment;

            var application =
                enrollment.InternshipApplication;

            var internship =
                application.Internship;

            context.Notifications.Add(
                new Notification
                {
                    UserId =
                        application.Student.UserId,

                    Title =
                        "Weekly Report Fully Approved",

                    Message =
                        $"Your week {report.WeekNumber} report " +
                        $"for '{internship.Title}' received " +
                        $"final university approval.",

                    IsRead = false,
                    CreatedAt = currentDate
                });

            context.Notifications.Add(
                new Notification
                {
                    UserId =
                        internship.Company.UserId,

                    Title =
                        "Weekly Report University Approved",

                    Message =
                        $"Week {report.WeekNumber} report for " +
                        $"{application.Student.FullName} " +
                        $"received final university approval.",

                    IsRead = false,
                    CreatedAt = currentDate
                });

            var trainingCompleted =
                await TryCompleteTrainingAsync(
                    enrollment,
                    report.Id,
                    supervisorUserId,
                    currentDate);

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                trainingCompleted
                    ? "Report approved and the training was completed successfully."
                    : "Weekly report received final approval.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "UniversitySupervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupervisorReturn(
            int id,
            string? feedback)
        {
            var supervisorUserId =
                userManager.GetUserId(User);

            var report =
                await GetSupervisorReportAsync(
                    id,
                    supervisorUserId);

            if (report == null)
            {
                return NotFound();
            }

            if (report.Status !=
                WeeklyReportStatus.CompanyApproved)
            {
                TempData["ErrorMessage"] =
                    "Only company-approved reports can be returned.";

                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(feedback))
            {
                TempData["ErrorMessage"] =
                    "Please write the reason for returning the report.";

                return RedirectToAction(nameof(Index));
            }

            report.Status =
                WeeklyReportStatus.SupervisorReturned;

            report.SupervisorFeedback =
                feedback.Trim();

            report.SupervisorRating = null;
            report.SupervisorReviewedAt = DateTime.Now;

            var enrollment =
                report.TrainingEnrollment;

            var application =
                enrollment.InternshipApplication;

            var internship =
                application.Internship;

            context.Notifications.Add(
                new Notification
                {
                    UserId =
                        application.Student.UserId,

                    Title =
                        "Weekly Report Needs Revision",

                    Message =
                        $"Your week {report.WeekNumber} report " +
                        $"was returned by the university supervisor. " +
                        $"Reason: {report.SupervisorFeedback}",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            context.Notifications.Add(
                new Notification
                {
                    UserId =
                        internship.Company.UserId,

                    Title =
                        "Weekly Report Returned by University",

                    Message =
                        $"Week {report.WeekNumber} report for " +
                        $"{application.Student.FullName} " +
                        $"requires revision.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Weekly report returned for revision.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> TryCompleteTrainingAsync(
            TrainingEnrollment enrollment,
            int approvedReportId,
            string? supervisorUserId,
            DateTime completedAt)
        {
            if (enrollment.Status !=
                TrainingStatus.Active)
            {
                return false;
            }

            var approvedHours =
                await context.TrainingHourEntries
                    .Where(entry =>
                        entry.TrainingEnrollmentId ==
                            enrollment.Id &&
                        entry.Status ==
                            TrainingHourStatus.Approved)
                    .SumAsync(entry =>
                        (decimal?)entry.Hours) ?? 0;

            if (approvedHours <
                enrollment.RequiredHours)
            {
                return false;
            }

            var hasAnotherUnapprovedReport =
                await context.WeeklyReports
                    .AnyAsync(report =>
                        report.TrainingEnrollmentId ==
                            enrollment.Id &&
                        report.Id != approvedReportId &&
                        report.Status !=
                            WeeklyReportStatus
                                .SupervisorApproved);

            if (hasAnotherUnapprovedReport)
            {
                return false;
            }

            var reportCount =
                await context.WeeklyReports
                    .CountAsync(report =>
                        report.TrainingEnrollmentId ==
                            enrollment.Id);

            if (reportCount == 0)
            {
                return false;
            }

            enrollment.Status =
                TrainingStatus.Completed;

            enrollment.CompletedAt =
                completedAt;

            enrollment.InternshipApplication.Status =
                ApplicationStatus.Completed;

            enrollment.InternshipApplication.ReviewedAt =
                completedAt;

            context.ApplicationStatusHistories.Add(
                new ApplicationStatusHistory
                {
                    InternshipApplicationId =
                        enrollment.InternshipApplicationId,

                    PreviousStatus =
                        ApplicationStatus.Accepted,

                    NewStatus =
                        ApplicationStatus.Completed,

                    Note =
                        "Training completed after fulfilling the required hours and receiving final report approval.",

                    ChangedAt =
                        completedAt,

                    ChangedByUserId =
                        supervisorUserId
                });

            context.Notifications.Add(
                new Notification
                {
                    UserId =
                        enrollment
                            .InternshipApplication
                            .Student
                            .UserId,

                    Title =
                        "Training Successfully Completed",

                    Message =
                        $"Congratulations! You completed " +
                        $"'{enrollment.InternshipApplication.Internship.Title}' " +
                        $"after fulfilling {approvedHours:0.0} " +
                        $"verified training hours.",

                    IsRead = false,
                    CreatedAt = completedAt
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

                    Title =
                        "Student Training Completed",

                    Message =
                        $"{enrollment.InternshipApplication.Student.FullName} " +
                        $"successfully completed the training requirements for " +
                        $"'{enrollment.InternshipApplication.Internship.Title}'.",

                    IsRead = false,
                    CreatedAt = completedAt
                });

            return true;
        }

        private async Task<WeeklyReport?>
            GetCompanyReportAsync(
                int reportId,
                string? companyUserId)
        {
            return await context.WeeklyReports
                .Include(report =>
                    report.TrainingEnrollment)
                .ThenInclude(enrollment =>
                    enrollment.InternshipApplication)
                .ThenInclude(application =>
                    application.Student)
                .Include(report =>
                    report.TrainingEnrollment)
                .ThenInclude(enrollment =>
                    enrollment.InternshipApplication)
                .ThenInclude(application =>
                    application.Internship)
                .ThenInclude(internship =>
                    internship.Company)
                .FirstOrDefaultAsync(report =>
                    report.Id == reportId &&
                    report
                        .TrainingEnrollment
                        .InternshipApplication
                        .Internship
                        .Company
                        .UserId == companyUserId);
        }

        private async Task<WeeklyReport?>
            GetSupervisorReportAsync(
                int reportId,
                string? supervisorUserId)
        {
            return await context.WeeklyReports
                .Include(report =>
                    report.TrainingEnrollment)
                .ThenInclude(enrollment =>
                    enrollment.InternshipApplication)
                .ThenInclude(application =>
                    application.Student)
                .Include(report =>
                    report.TrainingEnrollment)
                .ThenInclude(enrollment =>
                    enrollment.InternshipApplication)
                .ThenInclude(application =>
                    application.Internship)
                .ThenInclude(internship =>
                    internship.Company)
                .FirstOrDefaultAsync(report =>
                    report.Id == reportId &&
                    report
                        .TrainingEnrollment
                        .UniversitySupervisorUserId ==
                            supervisorUserId);
        }
    }
}