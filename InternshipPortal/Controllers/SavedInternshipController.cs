using InternshipPortal.Data;
using InternshipPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Student")]
    public class SavedInternshipController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SavedInternshipController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return NotFound("Student profile was not found.");
            }

            var savedInternships = await _context.SavedInternships
                .Include(saved => saved.Internship)
                    .ThenInclude(internship => internship.Company)
                .Where(saved => saved.StudentId == student.Id)
                .OrderByDescending(saved => saved.SavedAt)
                .ToListAsync();

            return View(savedInternships);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(
            int internshipId,
            string? returnUrl)
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return NotFound("Student profile was not found.");
            }

            var internshipExists = await _context.Internships
                .AnyAsync(internship =>
                    internship.Id == internshipId);

            if (!internshipExists)
            {
                return NotFound();
            }

            var alreadySaved = await _context.SavedInternships
                .AnyAsync(saved =>
                    saved.StudentId == student.Id &&
                    saved.InternshipId == internshipId);

            if (!alreadySaved)
            {
                var savedInternship = new SavedInternship
                {
                    StudentId = student.Id,
                    InternshipId = internshipId,
                    SavedAt = DateTime.Now
                };

                _context.SavedInternships.Add(savedInternship);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Internship saved successfully.";
            }
            else
            {
                TempData["InfoMessage"] =
                    "This internship is already saved.";
            }

            return RedirectSafely(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(
            int internshipId,
            string? returnUrl)
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return NotFound("Student profile was not found.");
            }

            var savedInternship = await _context.SavedInternships
                .FirstOrDefaultAsync(saved =>
                    saved.StudentId == student.Id &&
                    saved.InternshipId == internshipId);

            if (savedInternship != null)
            {
                _context.SavedInternships.Remove(savedInternship);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Internship removed from saved internships.";
            }

            return RedirectSafely(returnUrl);
        }

        private async Task<Student?> GetCurrentStudentAsync()
        {
            var userId = _userManager.GetUserId(User);

            return await _context.Students
                .FirstOrDefaultAsync(student =>
                    student.UserId == userId);
        }

        private IActionResult RedirectSafely(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}