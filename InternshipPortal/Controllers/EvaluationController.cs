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
    public class EvaluationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EvaluationController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Company")]
        [HttpGet]
        public async Task<IActionResult> Create(int applicationId)
        {
            var userId = _userManager.GetUserId(User);

            var application = await _context.InternshipApplications
                .Include(a => a.Student)
                .Include(a => a.Internship)
                    .ThenInclude(i => i.Company)
                .Include(a => a.Evaluation)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
            {
                return NotFound();
            }

            if (application.Internship.Company.UserId != userId)
            {
                return Forbid();
            }

            if (application.Status != ApplicationStatus.Accepted)
            {
                TempData["ErrorMessage"] =
                    "Only accepted applications can be evaluated.";

                return RedirectToAction(
                    "Applicants",
                    "Application",
                    new { internshipId = application.InternshipId });
            }

            if (application.Evaluation != null)
            {
                TempData["ErrorMessage"] =
                    "This student has already been evaluated.";

                return RedirectToAction(
                    "Applicants",
                    "Application",
                    new { internshipId = application.InternshipId });
            }

            var viewModel = new EvaluationViewModel
            {
                InternshipApplicationId = application.Id,
                StudentName = application.Student.FullName,
                InternshipTitle = application.Internship.Title
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EvaluationViewModel viewModel)
        {
            var userId = _userManager.GetUserId(User);

            var application = await _context.InternshipApplications
                .Include(a => a.Student)
                .Include(a => a.Internship)
                    .ThenInclude(i => i.Company)
                .Include(a => a.Evaluation)
                .FirstOrDefaultAsync(
                    a => a.Id == viewModel.InternshipApplicationId);

            if (application == null)
            {
                return NotFound();
            }

            if (application.Internship.Company.UserId != userId)
            {
                return Forbid();
            }

            viewModel.StudentName = application.Student.FullName;
            viewModel.InternshipTitle = application.Internship.Title;

            if (application.Status != ApplicationStatus.Accepted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Only accepted applications can be evaluated.");
            }

            if (application.Evaluation != null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This student has already been evaluated.");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var evaluation = new Evaluation
            {
                Rating = viewModel.Rating,
                Feedback = viewModel.Feedback,
                EvaluationDate = DateTime.Now,
                InternshipApplicationId = application.Id
            };

            application.Status = ApplicationStatus.Completed;
            application.ReviewedAt = DateTime.Now;

            var notification = new Notification
            {
                Title = "Training Completed",
                Message =
                    $"Your training in {application.Internship.Title} has been completed. " +
                    $"You received a rating of {viewModel.Rating} out of 5.",
                IsRead = false,
                CreatedAt = DateTime.Now,
                UserId = application.Student.UserId
            };

            _context.Evaluations.Add(evaluation);
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The student evaluation has been submitted successfully.";

            return RedirectToAction(
                "Applicants",
                "Application",
                new { internshipId = application.InternshipId });
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> MyEvaluations()
        {
            var userId = _userManager.GetUserId(User);

            var evaluations = await _context.Evaluations
                .Include(e => e.InternshipApplication)
                    .ThenInclude(a => a.Student)
                .Include(e => e.InternshipApplication)
                    .ThenInclude(a => a.Internship)
                        .ThenInclude(i => i.Company)
                .Where(e =>
                    e.InternshipApplication.Student.UserId == userId)
                .OrderByDescending(e => e.EvaluationDate)
                .ToListAsync();

            return View(evaluations);
        }
    }
}