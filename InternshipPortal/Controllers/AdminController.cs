using InternshipPortal.Data;
using InternshipPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext context;

        public AdminController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> PendingInternships()
        {
            var internships = await context.Internships
                .Include(i => i.Company)
                .Where(i => !i.IsApproved && i.IsActive)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(internships);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveInternship(int id)
        {
            var internship = await context.Internships
                .Include(i => i.Company)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (internship == null)
            {
                return NotFound();
            }

            internship.IsApproved = true;
            internship.IsActive = true;

            var notification = new Notification
            {
                Title = "Internship Approved",
                Message =
                    $"Your internship '{internship.Title}' has been approved and published.",
                UserId = internship.Company.UserId,
                CreatedAt = DateTime.Now
            };

            context.Notifications.Add(notification);

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Internship approved and published successfully.";

            return RedirectToAction(nameof(PendingInternships));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectInternship(int id)
        {
            var internship = await context.Internships
                .Include(i => i.Company)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (internship == null)
            {
                return NotFound();
            }

            internship.IsApproved = false;
            internship.IsActive = false;

            var notification = new Notification
            {
                Title = "Internship Rejected",
                Message =
                    $"Your internship '{internship.Title}' has been rejected. You can edit it and submit it again.",
                UserId = internship.Company.UserId,
                CreatedAt = DateTime.Now
            };

            context.Notifications.Add(notification);

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Internship rejected successfully.";

            return RedirectToAction(nameof(PendingInternships));
        }

        public async Task<IActionResult> AllInternships()
        {
            var internships = await context.Internships
                .Include(i => i.Company)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(internships);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeActiveStatus(int id)
        {
            var internship = await context.Internships
                .FirstOrDefaultAsync(i => i.Id == id);

            if (internship == null)
            {
                return NotFound();
            }

            internship.IsActive = !internship.IsActive;

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Internship status updated successfully.";

            return RedirectToAction(nameof(AllInternships));
        }
    }
}