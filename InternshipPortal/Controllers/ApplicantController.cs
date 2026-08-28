using InternshipPortal.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Company")]
    public class ApplicantController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public ApplicantController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Details(int applicationId)
        {
            var userId = userManager.GetUserId(User);

            var application =
                await context.InternshipApplications
                    .Include(a => a.Student)
                    .Include(a => a.Internship)
                    .ThenInclude(i => i.Company)
                    .FirstOrDefaultAsync(a =>
                        a.Id == applicationId &&
                        a.Internship.Company.UserId == userId);

            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }
    }
}