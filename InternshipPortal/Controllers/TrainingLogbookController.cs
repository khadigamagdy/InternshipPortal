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
    public class TrainingLogbookController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public TrainingLogbookController(
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
            var userId = userManager.GetUserId(User);

            var student = await context.Students
                .FirstOrDefaultAsync(student =>
                    student.UserId == userId);

            if (student == null)
            {
                TempData["ErrorMessage"] =
                    "Please complete your student profile first.";

                return RedirectToAction(
                    "Profile",
                    "Student");
            }

            var enrollments = await context.TrainingEnrollments
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
                    enrollment.InternshipApplication.StudentId ==
                        student.Id)
                .OrderByDescending(enrollment =>
                    enrollment.CreatedAt)
                .ToListAsync();

            return View(enrollments);
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> AddHours(
            int enrollmentId)
        {
            var enrollment =
                await GetStudentEnrollmentAsync(
                    enrollmentId);

            if (enrollment == null)
            {
                return NotFound();
            }

            if (enrollment.Status != TrainingStatus.Active)
            {
                TempData["ErrorMessage"] =
                    "Training hours can only be added after the training is approved and activated.";

                return RedirectToAction(nameof(Index));
            }

            var model = new TrainingHourEntryViewModel
            {
                TrainingEnrollmentId = enrollment.Id,

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

                TrainingDate = DateTime.Today
            };

            return View(model);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHours(
            TrainingHourEntryViewModel model)
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

            if (enrollment.Status != TrainingStatus.Active)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This training is not active.");
            }

            if (model.TrainingDate.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.TrainingDate),
                    "You cannot register future training hours.");
            }

            if (model.TrainingDate.Date <
                enrollment.StartDate.Date)
            {
                ModelState.AddModelError(
                    nameof(model.TrainingDate),
                    "The training date cannot be before the training start date.");
            }

            if (model.TrainingDate.Date >
                enrollment.ExpectedEndDate.Date)
            {
                ModelState.AddModelError(
                    nameof(model.TrainingDate),
                    "The training date cannot be after the expected end date.");
            }

            var registeredHoursForDate =
                await context.TrainingHourEntries
                    .Where(entry =>
                        entry.TrainingEnrollmentId ==
                            enrollment.Id &&
                        entry.TrainingDate.Date ==
                            model.TrainingDate.Date &&
                        entry.Status !=
                            TrainingHourStatus.Rejected)
                    .SumAsync(entry =>
                        (decimal?)entry.Hours) ?? 0;

            if (registeredHoursForDate + model.Hours > 24)
            {
                ModelState.AddModelError(
                    nameof(model.Hours),
                    "The total registered hours for this date cannot exceed 24 hours.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var hourEntry = new TrainingHourEntry
            {
                TrainingEnrollmentId =
                    enrollment.Id,

                TrainingDate =
                    model.TrainingDate.Date,

                Hours = model.Hours,

                TaskTitle =
                    model.TaskTitle.Trim(),

                TaskDescription =
                    model.TaskDescription.Trim(),

                LearnedSkills =
                    string.IsNullOrWhiteSpace(
                        model.LearnedSkills)
                        ? null
                        : model.LearnedSkills.Trim(),

                Status =
                    TrainingHourStatus.Pending,

                CreatedAt = DateTime.Now
            };

            context.TrainingHourEntries.Add(
                hourEntry);

            var companyUserId =
                enrollment
                    .InternshipApplication
                    .Internship
                    .Company
                    .UserId;

            context.Notifications.Add(
                new Notification
                {
                    UserId = companyUserId,

                    Title = "Training Hours Submitted",

                    Message =
                        $"{enrollment.InternshipApplication.Student.FullName} " +
                        $"submitted {model.Hours:0.0} training hours " +
                        $"for {model.TrainingDate:dd MMM yyyy}.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Training hours submitted successfully and sent to the company for approval.";

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
                .Include(enrollment =>
                    enrollment.HourEntries)
                .FirstOrDefaultAsync(enrollment =>
                    enrollment.Id == enrollmentId &&
                    enrollment
                        .InternshipApplication
                        .Student
                        .UserId == userId);
        }
    }
}