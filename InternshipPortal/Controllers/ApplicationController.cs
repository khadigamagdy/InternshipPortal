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
    public class ApplicationController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ApplicationController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            this.context = context;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Apply(int internshipId)
        {
            var userId = userManager.GetUserId(User);

            var student = await context.Students
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

            if (internship.ApplicationDeadline < DateTime.Today)
            {
                TempData["Error"] =
                    "The application deadline has passed.";

                return RedirectToAction(
                    "Details",
                    "Internship",
                    new { id = internshipId });
            }

            var alreadyApplied =
                await context.InternshipApplications
                    .AnyAsync(application =>
                        application.StudentId == student.Id &&
                        application.InternshipId == internshipId);

            if (alreadyApplied)
            {
                TempData["Error"] =
                    "You have already applied for this internship.";

                return RedirectToAction(nameof(MyApplications));
            }

            var model = new ApplicationViewModel
            {
                InternshipId = internship.Id,
                InternshipTitle = internship.Title,
                CompanyName = internship.Company.Name,
                CurrentCVPath = student.CVPath
            };

            return View(model);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(
            ApplicationViewModel model)
        {
            var userId = userManager.GetUserId(User);

            var student = await context.Students
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

            var internship = await context.Internships
                .Include(internship => internship.Company)
                .FirstOrDefaultAsync(internship =>
                    internship.Id == model.InternshipId &&
                    internship.IsApproved &&
                    internship.IsActive);

            if (internship == null)
            {
                return NotFound();
            }

            model.InternshipTitle = internship.Title;
            model.CompanyName = internship.Company.Name;
            model.CurrentCVPath = student.CVPath;

            if (internship.ApplicationDeadline < DateTime.Today)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The application deadline has passed.");
            }

            var alreadyApplied =
                await context.InternshipApplications
                    .AnyAsync(application =>
                        application.StudentId == student.Id &&
                        application.InternshipId ==
                            model.InternshipId);

            if (alreadyApplied)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "You have already applied for this internship.");
            }

            if (model.CVFile != null)
            {
                var extension = Path
                    .GetExtension(model.CVFile.FileName)
                    .ToLower();

                if (extension != ".pdf")
                {
                    ModelState.AddModelError(
                        nameof(model.CVFile),
                        "Only PDF files are allowed.");
                }

                if (model.CVFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        nameof(model.CVFile),
                        "The CV must not exceed 5 MB.");
                }
            }

            if (model.CVFile == null &&
                string.IsNullOrWhiteSpace(student.CVPath))
            {
                ModelState.AddModelError(
                    nameof(model.CVFile),
                    "Please upload a CV before applying.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cvPath = student.CVPath;

            if (model.CVFile != null)
            {
                var uploadsFolder = Path.Combine(
                    webHostEnvironment.WebRootPath,
                    "uploads",
                    "application-cvs");

                Directory.CreateDirectory(uploadsFolder);

                var fileName =
                    $"{Guid.NewGuid()}" +
                    $"{Path.GetExtension(model.CVFile.FileName)}";

                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName);

                using var stream = new FileStream(
                    filePath,
                    FileMode.Create);

                await model.CVFile.CopyToAsync(stream);

                cvPath =
                    $"/uploads/application-cvs/{fileName}";
            }

            var application = new InternshipApplication
            {
                StudentId = student.Id,
                InternshipId = model.InternshipId,
                CoverLetter = model.CoverLetter,
                CVPath = cvPath,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.Now
            };

            application.StatusHistory.Add(
                new ApplicationStatusHistory
                {
                    PreviousStatus = null,
                    NewStatus = ApplicationStatus.Pending,
                    Note = "The student submitted the application.",
                    ChangedAt = DateTime.Now,
                    ChangedByUserId = userId
                });

            context.InternshipApplications.Add(application);

            context.Notifications.Add(
                new Notification
                {
                    Title = "New Internship Application",

                    Message =
                        $"{student.FullName} applied for " +
                        $"'{internship.Title}'.",

                    UserId = internship.Company.UserId,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Your application has been submitted successfully.";

            return RedirectToAction(nameof(MyApplications));
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            var userId = userManager.GetUserId(User);

            var student = await context.Students
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

            var applications =
                await context.InternshipApplications
                    .Include(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                    .Include(application =>
                        application.Interviews)
                    .Include(application =>
                        application.StatusHistory)
                    .Include(application =>
                        application.TrainingEnrollment)
                    .Where(application =>
                        application.StudentId == student.Id)
                    .OrderByDescending(application =>
                        application.AppliedAt)
                    .ToListAsync();

            return View(applications);
        }

        [Authorize(Roles = "Company")]
        [HttpGet]
        public async Task<IActionResult> Applicants(
            int internshipId)
        {
            var userId = userManager.GetUserId(User);

            var internship = await context.Internships
                .Include(internship => internship.Company)
                .FirstOrDefaultAsync(internship =>
                    internship.Id == internshipId &&
                    internship.Company.UserId == userId);

            if (internship == null)
            {
                return NotFound();
            }

            var applications =
                await context.InternshipApplications
                    .Include(application =>
                        application.Student)
                    .Include(application =>
                        application.Internship)
                    .Include(application =>
                        application.Interviews)
                    .Include(application =>
                        application.StatusHistory)
                    .ThenInclude(history =>
                        history.ChangedByUser)
                    .Include(application =>
                        application.TrainingEnrollment)
                    .Where(application =>
                        application.InternshipId ==
                            internshipId)
                    .OrderByDescending(application =>
                        application.AppliedAt)
                    .ToListAsync();

            ViewBag.Internship = internship;

            return View(applications);
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartReview(int id)
        {
            var userId = userManager.GetUserId(User);

            var application =
                await GetCompanyApplicationAsync(
                    id,
                    userId);

            if (application == null)
            {
                return NotFound();
            }

            if (application.Status !=
                ApplicationStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "Only pending applications can be moved to review.";

                return RedirectToApplicants(application);
            }

            var previousStatus = application.Status;

            application.Status =
                ApplicationStatus.UnderReview;

            application.ReviewedAt = DateTime.Now;

            AddStatusHistory(
                application,
                previousStatus,
                ApplicationStatus.UnderReview,
                "The company started reviewing the application.",
                userId);

            context.Notifications.Add(
                new Notification
                {
                    UserId = application.Student.UserId,
                    Title = "Application Under Review",

                    Message =
                        $"Your application for " +
                        $"'{application.Internship.Title}' " +
                        $"is now being reviewed.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The application is now under review.";

            return RedirectToApplicants(application);
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var userId = userManager.GetUserId(User);

            var application =
                await GetCompanyApplicationAsync(
                    id,
                    userId);

            if (application == null)
            {
                return NotFound();
            }

            var allowedStatuses = new[]
            {
                ApplicationStatus.Pending,
                ApplicationStatus.UnderReview,
                ApplicationStatus.InterviewScheduled
            };

            if (!allowedStatuses.Contains(application.Status))
            {
                TempData["ErrorMessage"] =
                    "This application cannot be accepted.";

                return RedirectToApplicants(application);
            }

            if (application.Internship.AvailablePositions <= 0)
            {
                TempData["ErrorMessage"] =
                    "There are no available positions remaining.";

                return RedirectToApplicants(application);
            }

            if (application.TrainingEnrollment != null)
            {
                TempData["ErrorMessage"] =
                    "A training enrollment already exists for this application.";

                return RedirectToApplicants(application);
            }

            var previousStatus = application.Status;
            var currentDate = DateTime.Now;

            application.Status =
                ApplicationStatus.Accepted;

            application.ReviewedAt = currentDate;

            application.Internship.AvailablePositions--;

            AddStatusHistory(
                application,
                previousStatus,
                ApplicationStatus.Accepted,
                "The company accepted the application and created the training enrollment.",
                userId);

            var startDate =
                application.Internship.StartDate;

            if (startDate.Date < DateTime.Today)
            {
                startDate = DateTime.Today;
            }

            var expectedEndDate =
                application.Internship.EndDate;

            if (expectedEndDate.Date < startDate.Date)
            {
                expectedEndDate =
                    startDate.AddMonths(3);
            }

            var trainingEnrollment =
                new TrainingEnrollment
                {
                    InternshipApplicationId =
                        application.Id,

                    StartDate = startDate,

                    ExpectedEndDate =
                        expectedEndDate,

                    RequiredHours = 120,

                    Status =
                        TrainingStatus
                            .PendingUniversityApproval,

                    CreatedAt = currentDate
                };

            context.TrainingEnrollments.Add(
                trainingEnrollment);

            context.Notifications.Add(
                new Notification
                {
                    Title = "Application Accepted",

                    Message =
                        $"Congratulations! Your application for " +
                        $"'{application.Internship.Title}' " +
                        $"has been accepted. Your training record " +
                        $"was created and is waiting for approval.",

                    UserId = application.Student.UserId,
                    IsRead = false,
                    CreatedAt = currentDate
                });

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Student accepted and training enrollment created successfully.";

            return RedirectToApplicants(application);
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var userId = userManager.GetUserId(User);

            var application =
                await GetCompanyApplicationAsync(
                    id,
                    userId);

            if (application == null)
            {
                return NotFound();
            }

            var allowedStatuses = new[]
            {
                ApplicationStatus.Pending,
                ApplicationStatus.UnderReview,
                ApplicationStatus.InterviewScheduled
            };

            if (!allowedStatuses.Contains(application.Status))
            {
                TempData["ErrorMessage"] =
                    "This application cannot be rejected.";

                return RedirectToApplicants(application);
            }

            var previousStatus = application.Status;

            application.Status =
                ApplicationStatus.Rejected;

            application.ReviewedAt = DateTime.Now;

            AddStatusHistory(
                application,
                previousStatus,
                ApplicationStatus.Rejected,
                "The company rejected the application.",
                userId);

            foreach (var interview in application.Interviews)
            {
                if (interview.Status ==
                        InterviewStatus.Pending ||
                    interview.Status ==
                        InterviewStatus.AcceptedByStudent)
                {
                    interview.Status =
                        InterviewStatus.Cancelled;
                }
            }

            context.Notifications.Add(
                new Notification
                {
                    Title = "Application Status Updated",

                    Message =
                        $"Your application for " +
                        $"'{application.Internship.Title}' " +
                        $"was not accepted. Keep exploring " +
                        $"other opportunities.",

                    UserId = application.Student.UserId,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Student application rejected successfully.";

            return RedirectToApplicants(application);
        }

        private async Task<InternshipApplication?>
            GetCompanyApplicationAsync(
                int applicationId,
                string? companyUserId)
        {
            return await context.InternshipApplications
                .Include(application =>
                    application.Student)
                .Include(application =>
                    application.Internship)
                .ThenInclude(internship =>
                    internship.Company)
                .Include(application =>
                    application.Interviews)
                .Include(application =>
                    application.StatusHistory)
                .Include(application =>
                    application.TrainingEnrollment)
                .FirstOrDefaultAsync(application =>
                    application.Id == applicationId &&
                    application.Internship.Company.UserId ==
                        companyUserId);
        }

        private void AddStatusHistory(
            InternshipApplication application,
            ApplicationStatus previousStatus,
            ApplicationStatus newStatus,
            string note,
            string? changedByUserId)
        {
            context.ApplicationStatusHistories.Add(
                new ApplicationStatusHistory
                {
                    InternshipApplicationId =
                        application.Id,

                    PreviousStatus = previousStatus,
                    NewStatus = newStatus,
                    Note = note,
                    ChangedAt = DateTime.Now,
                    ChangedByUserId = changedByUserId
                });
        }

        private IActionResult RedirectToApplicants(
            InternshipApplication application)
        {
            return RedirectToAction(
                nameof(Applicants),
                new
                {
                    internshipId =
                        application.InternshipId
                });
        }
    }
}