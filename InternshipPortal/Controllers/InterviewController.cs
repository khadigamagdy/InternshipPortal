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
    public class InterviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public InterviewController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Company")]
        [HttpGet]
        public async Task<IActionResult> Schedule(int applicationId)
        {
            var currentUserId = _userManager.GetUserId(User);

            var application = await _context.InternshipApplications
                .Include(item => item.Student)
                .Include(item => item.Internship)
                    .ThenInclude(internship => internship.Company)
                .FirstOrDefaultAsync(item =>
                    item.Id == applicationId &&
                    item.Internship.Company.UserId == currentUserId);

            if (application == null)
            {
                return NotFound();
            }

            if (application.Status == ApplicationStatus.Rejected ||
                application.Status == ApplicationStatus.Completed)
            {
                TempData["ErrorMessage"] =
                    "An interview cannot be scheduled for this application.";

                return RedirectToAction(
                    "Applicants",
                    "Application",
                    new
                    {
                        internshipId = application.InternshipId
                    });
            }

            var viewModel = new ScheduleInterviewViewModel
            {
                InternshipApplicationId = application.Id,
                StudentName = application.Student.FullName,
                InternshipTitle = application.Internship.Title,
                ScheduledAt = DateTime.Now.AddDays(1),
                DurationMinutes = 30,
                Type = InterviewType.Online
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(
            ScheduleInterviewViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);

            var application = await _context.InternshipApplications
                .Include(item => item.Student)
                .Include(item => item.Internship)
                    .ThenInclude(internship => internship.Company)
                .FirstOrDefaultAsync(item =>
                    item.Id == model.InternshipApplicationId &&
                    item.Internship.Company.UserId == currentUserId);

            if (application == null)
            {
                return NotFound();
            }

            model.StudentName = application.Student.FullName;
            model.InternshipTitle = application.Internship.Title;

            if (model.ScheduledAt <= DateTime.Now)
            {
                ModelState.AddModelError(
                    nameof(model.ScheduledAt),
                    "The interview must be scheduled in the future.");
            }

            if (model.Type == InterviewType.Online &&
                string.IsNullOrWhiteSpace(model.MeetingLink))
            {
                ModelState.AddModelError(
                    nameof(model.MeetingLink),
                    "A meeting link is required for an online interview.");
            }

            if (model.Type == InterviewType.InPerson &&
                string.IsNullOrWhiteSpace(model.Location))
            {
                ModelState.AddModelError(
                    nameof(model.Location),
                    "A location is required for an in-person interview.");
            }

            if (application.Status == ApplicationStatus.Rejected ||
                application.Status == ApplicationStatus.Completed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An interview cannot be scheduled for this application.");
            }

            var conflictingInterview = await _context.Interviews
                .AnyAsync(interview =>
                    interview.InternshipApplicationId ==
                        model.InternshipApplicationId &&
                    interview.ScheduledAt == model.ScheduledAt &&
                    interview.Status != InterviewStatus.Cancelled &&
                    interview.Status !=
                        InterviewStatus.DeclinedByStudent);

            if (conflictingInterview)
            {
                ModelState.AddModelError(
                    nameof(model.ScheduledAt),
                    "An interview already exists at this date and time.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var previousStatus = application.Status;

            var interview = new Interview
            {
                InternshipApplicationId =
                    model.InternshipApplicationId,

                ScheduledAt = model.ScheduledAt,
                DurationMinutes = model.DurationMinutes,
                Type = model.Type,
                MeetingLink = model.MeetingLink?.Trim(),
                Location = model.Location?.Trim(),
                Notes = model.Notes?.Trim(),

                Status = InterviewStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.Interviews.Add(interview);

            application.Status =
                ApplicationStatus.InterviewScheduled;

            application.ReviewedAt ??= DateTime.Now;

            _context.ApplicationStatusHistories.Add(
                new ApplicationStatusHistory
                {
                    InternshipApplicationId = application.Id,
                    PreviousStatus = previousStatus,
                    NewStatus =
                        ApplicationStatus.InterviewScheduled,

                    Note =
                        $"An interview was scheduled for " +
                        $"{model.ScheduledAt:dd MMM yyyy, hh:mm tt}.",

                    ChangedAt = DateTime.Now,
                    ChangedByUserId = currentUserId
                });

            _context.Notifications.Add(
                new Notification
                {
                    UserId = application.Student.UserId,
                    Title = "New Interview Scheduled",

                    Message =
                        $"{application.Internship.Company.Name} " +
                        $"scheduled an interview for " +
                        $"{application.Internship.Title} on " +
                        $"{model.ScheduledAt:dd MMM yyyy at hh:mm tt}.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The interview has been scheduled successfully.";

            return RedirectToAction(
                "Applicants",
                "Application",
                new
                {
                    internshipId = application.InternshipId
                });
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> MyInterviews()
        {
            var currentUserId = _userManager.GetUserId(User);

            var studentId = await _context.Students
                .Where(student =>
                    student.UserId == currentUserId)
                .Select(student => (int?)student.Id)
                .FirstOrDefaultAsync();

            if (!studentId.HasValue)
            {
                return NotFound("Student profile was not found.");
            }

            var interviews = await _context.Interviews
                .Include(interview =>
                    interview.InternshipApplication)
                    .ThenInclude(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                .Where(interview =>
                    interview.InternshipApplication.StudentId ==
                        studentId.Value)
                .OrderByDescending(interview =>
                    interview.ScheduledAt)
                .ToListAsync();

            return View(interviews);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var interview = await _context.Interviews
                .Include(item => item.InternshipApplication)
                    .ThenInclude(application => application.Student)
                .Include(item => item.InternshipApplication)
                    .ThenInclude(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.InternshipApplication.Student.UserId ==
                        currentUserId);

            if (interview == null)
            {
                return NotFound();
            }

            if (interview.Status != InterviewStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "This interview has already been answered.";

                return RedirectToAction(nameof(MyInterviews));
            }

            if (interview.ScheduledAt <= DateTime.Now)
            {
                TempData["ErrorMessage"] =
                    "This interview date has already passed.";

                return RedirectToAction(nameof(MyInterviews));
            }

            interview.Status =
                InterviewStatus.AcceptedByStudent;

            interview.RespondedAt = DateTime.Now;

            _context.Notifications.Add(
                new Notification
                {
                    UserId = interview
                        .InternshipApplication
                        .Internship
                        .Company
                        .UserId,

                    Title = "Interview Accepted",

                    Message =
                        $"{interview.InternshipApplication.Student.FullName} " +
                        $"accepted the interview for " +
                        $"{interview.InternshipApplication.Internship.Title}.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The interview has been accepted successfully.";

            return RedirectToAction(nameof(MyInterviews));
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var interview = await _context.Interviews
                .Include(item => item.InternshipApplication)
                    .ThenInclude(application => application.Student)
                .Include(item => item.InternshipApplication)
                    .ThenInclude(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.InternshipApplication.Student.UserId ==
                        currentUserId);

            if (interview == null)
            {
                return NotFound();
            }

            if (interview.Status != InterviewStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "This interview has already been answered.";

                return RedirectToAction(nameof(MyInterviews));
            }

            var application =
                interview.InternshipApplication;

            var previousStatus = application.Status;

            interview.Status =
                InterviewStatus.DeclinedByStudent;

            interview.RespondedAt = DateTime.Now;

            application.Status =
                ApplicationStatus.UnderReview;

            _context.ApplicationStatusHistories.Add(
                new ApplicationStatusHistory
                {
                    InternshipApplicationId = application.Id,
                    PreviousStatus = previousStatus,
                    NewStatus = ApplicationStatus.UnderReview,

                    Note =
                        "The student declined the scheduled interview.",

                    ChangedAt = DateTime.Now,
                    ChangedByUserId = currentUserId
                });

            _context.Notifications.Add(
                new Notification
                {
                    UserId =
                        application.Internship.Company.UserId,

                    Title = "Interview Declined",

                    Message =
                        $"{application.Student.FullName} declined " +
                        $"the interview for " +
                        $"{application.Internship.Title}.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The interview has been declined.";

            return RedirectToAction(nameof(MyInterviews));
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var interview = await _context.Interviews
                .Include(item => item.InternshipApplication)
                    .ThenInclude(application => application.Student)
                .Include(item => item.InternshipApplication)
                    .ThenInclude(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.InternshipApplication
                        .Internship
                        .Company
                        .UserId == currentUserId);

            if (interview == null)
            {
                return NotFound();
            }

            if (interview.Status == InterviewStatus.Completed ||
                interview.Status == InterviewStatus.Cancelled)
            {
                TempData["ErrorMessage"] =
                    "This interview cannot be cancelled.";

                return RedirectToAction(
                    "Applicants",
                    "Application",
                    new
                    {
                        internshipId =
                            interview.InternshipApplication
                                .InternshipId
                    });
            }

            var application =
                interview.InternshipApplication;

            var previousStatus = application.Status;

            interview.Status =
                InterviewStatus.Cancelled;

            application.Status =
                ApplicationStatus.UnderReview;

            _context.ApplicationStatusHistories.Add(
                new ApplicationStatusHistory
                {
                    InternshipApplicationId = application.Id,
                    PreviousStatus = previousStatus,
                    NewStatus = ApplicationStatus.UnderReview,

                    Note =
                        "The company cancelled the scheduled interview.",

                    ChangedAt = DateTime.Now,
                    ChangedByUserId = currentUserId
                });

            _context.Notifications.Add(
                new Notification
                {
                    UserId = application.Student.UserId,
                    Title = "Interview Cancelled",

                    Message =
                        $"{application.Internship.Company.Name} " +
                        $"cancelled the interview for " +
                        $"{application.Internship.Title}.",

                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The interview has been cancelled.";

            return RedirectToAction(
                "Applicants",
                "Application",
                new
                {
                    internshipId = application.InternshipId
                });
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var interview = await _context.Interviews
                .Include(item => item.InternshipApplication)
                    .ThenInclude(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.InternshipApplication
                        .Internship
                        .Company
                        .UserId == currentUserId);

            if (interview == null)
            {
                return NotFound();
            }

            if (interview.Status !=
                InterviewStatus.AcceptedByStudent)
            {
                TempData["ErrorMessage"] =
                    "Only an accepted interview can be completed.";

                return RedirectToAction(
                    "Applicants",
                    "Application",
                    new
                    {
                        internshipId =
                            interview.InternshipApplication
                                .InternshipId
                    });
            }

            interview.Status = InterviewStatus.Completed;
            interview.CompletedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The interview has been marked as completed.";

            return RedirectToAction(
                "Applicants",
                "Application",
                new
                {
                    internshipId =
                        interview.InternshipApplication
                            .InternshipId
                });
        }
    }
}