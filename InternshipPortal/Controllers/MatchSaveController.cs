using InternshipPortal.Data;
using InternshipPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Student")]
    public class MatchSaveController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public MatchSaveController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(
            int internshipId)
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

            var internshipExists =
                await context.Internships.AnyAsync(internship =>
                    internship.Id == internshipId &&
                    internship.IsApproved &&
                    internship.IsActive);

            if (!internshipExists)
            {
                return NotFound();
            }

            var savedInternship =
                await context.SavedInternships
                    .FirstOrDefaultAsync(saved =>
                        saved.StudentId == student.Id &&
                        saved.InternshipId == internshipId);

            if (savedInternship == null)
            {
                context.SavedInternships.Add(
                    new SavedInternship
                    {
                        StudentId = student.Id,
                        InternshipId = internshipId,
                        SavedAt = DateTime.Now
                    });

                TempData["SuccessMessage"] =
                    "Internship saved successfully.";
            }
            else
            {
                context.SavedInternships.Remove(
                    savedInternship);

                TempData["SuccessMessage"] =
                    "Internship removed from saved opportunities.";
            }

            await context.SaveChangesAsync();

            return RedirectToAction(
                "Index",
                "Matching");
        }
    }
}