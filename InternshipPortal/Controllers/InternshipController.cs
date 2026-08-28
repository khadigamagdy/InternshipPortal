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
    public class InternshipController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public InternshipController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index(
            string? search,
            string? location,
            WorkMode? workMode)
        {
            var internships = context.Internships
                .Include(i => i.Company)
                .Where(i => i.IsApproved && i.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                internships = internships.Where(i =>
                    i.Title.Contains(search) ||
                    i.Description.Contains(search) ||
                    i.RequiredSkills.Contains(search) ||
                    i.Company.Name.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                internships = internships.Where(i =>
                    i.Location.Contains(location));
            }

            if (workMode.HasValue)
            {
                internships = internships.Where(i =>
                    i.WorkMode == workMode.Value);
            }

            ViewBag.Search = search;
            ViewBag.Location = location;
            ViewBag.WorkMode = workMode;

            return View(await internships
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var internship = await context.Internships
                .Include(i => i.Company)
                .Include(i => i.Applications)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (internship == null)
            {
                return NotFound();
            }

            var userId = userManager.GetUserId(User);

            if (!internship.IsApproved &&
                !User.IsInRole("Admin") &&
                internship.Company.UserId != userId)
            {
                return Forbid();
            }

            return View(internship);
        }

        [Authorize(Roles = "Company")]
        public async Task<IActionResult> MyInternships()
        {
            var userId = userManager.GetUserId(User);

            var company = await context.Companies
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (company == null)
            {
                TempData["Error"] =
                    "Please complete your company profile first.";

                return RedirectToAction(
                    "Profile",
                    "Company");
            }

            var internships = await context.Internships
                .Where(i => i.CompanyId == company.Id)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(internships);
        }

        [Authorize(Roles = "Company")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = userManager.GetUserId(User);

            var companyExists = await context.Companies
                .AnyAsync(c => c.UserId == userId);

            if (!companyExists)
            {
                TempData["Error"] =
                    "Please complete your company profile first.";

                return RedirectToAction(
                    "Profile",
                    "Company");
            }

            var model = new InternshipFormViewModel
            {
                StartDate = DateTime.Today.AddDays(14),
                EndDate = DateTime.Today.AddMonths(2),
                ApplicationDeadline = DateTime.Today.AddDays(10),
                AvailablePositions = 1
            };

            return View(model);
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            InternshipFormViewModel model)
        {
            ValidateInternshipDates(model);

            if (model.IsPaid && model.Salary == null)
            {
                ModelState.AddModelError(
                    "Salary",
                    "Salary is required for a paid internship.");
            }

            if (!model.IsPaid)
            {
                model.Salary = null;
            }

            var userId = userManager.GetUserId(User);

            var company = await context.Companies
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (company == null)
            {
                TempData["Error"] =
                    "Please complete your company profile first.";

                return RedirectToAction(
                    "Profile",
                    "Company");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var internship = new Internship
            {
                Title = model.Title,
                Description = model.Description,
                RequiredSkills = model.RequiredSkills,
                Location = model.Location,
                WorkMode = model.WorkMode,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                ApplicationDeadline = model.ApplicationDeadline,
                AvailablePositions = model.AvailablePositions,
                IsPaid = model.IsPaid,
                Salary = model.Salary,
                IsApproved = false,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CompanyId = company.Id
            };

            context.Internships.Add(internship);
            await context.SaveChangesAsync();

            TempData["Success"] =
                "Internship created and sent for admin approval.";

            return RedirectToAction(nameof(MyInternships));
        }

        [Authorize(Roles = "Company")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = userManager.GetUserId(User);

            var internship = await context.Internships
                .Include(i => i.Company)
                .FirstOrDefaultAsync(i =>
                    i.Id == id &&
                    i.Company.UserId == userId);

            if (internship == null)
            {
                return NotFound();
            }

            var model = new InternshipFormViewModel
            {
                Id = internship.Id,
                Title = internship.Title,
                Description = internship.Description,
                RequiredSkills = internship.RequiredSkills,
                Location = internship.Location,
                WorkMode = internship.WorkMode,
                StartDate = internship.StartDate,
                EndDate = internship.EndDate,
                ApplicationDeadline =
                    internship.ApplicationDeadline,
                AvailablePositions =
                    internship.AvailablePositions,
                IsPaid = internship.IsPaid,
                Salary = internship.Salary
            };

            return View(model);
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            InternshipFormViewModel model)
        {
            ValidateInternshipDates(model);

            if (model.IsPaid && model.Salary == null)
            {
                ModelState.AddModelError(
                    "Salary",
                    "Salary is required for a paid internship.");
            }

            if (!model.IsPaid)
            {
                model.Salary = null;
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = userManager.GetUserId(User);

            var internship = await context.Internships
                .Include(i => i.Company)
                .FirstOrDefaultAsync(i =>
                    i.Id == model.Id &&
                    i.Company.UserId == userId);

            if (internship == null)
            {
                return NotFound();
            }

            internship.Title = model.Title;
            internship.Description = model.Description;
            internship.RequiredSkills = model.RequiredSkills;
            internship.Location = model.Location;
            internship.WorkMode = model.WorkMode;
            internship.StartDate = model.StartDate;
            internship.EndDate = model.EndDate;
            internship.ApplicationDeadline =
                model.ApplicationDeadline;
            internship.AvailablePositions =
                model.AvailablePositions;
            internship.IsPaid = model.IsPaid;
            internship.Salary = model.Salary;
            internship.IsApproved = false;

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Internship updated and sent for approval again.";

            return RedirectToAction(nameof(MyInternships));
        }

        [Authorize(Roles = "Company")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = userManager.GetUserId(User);

            var internship = await context.Internships
                .Include(i => i.Company)
                .FirstOrDefaultAsync(i =>
                    i.Id == id &&
                    i.Company.UserId == userId);

            if (internship == null)
            {
                return NotFound();
            }

            return View(internship);
        }

        [Authorize(Roles = "Company")]
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = userManager.GetUserId(User);

            var internship = await context.Internships
                .Include(i => i.Company)
                .FirstOrDefaultAsync(i =>
                    i.Id == id &&
                    i.Company.UserId == userId);

            if (internship == null)
            {
                return NotFound();
            }

            var hasApplications =
                await context.InternshipApplications
                    .AnyAsync(a => a.InternshipId == id);

            if (hasApplications)
            {
                TempData["Error"] =
                    "This internship cannot be deleted because it has applications.";

                return RedirectToAction(nameof(MyInternships));
            }

            context.Internships.Remove(internship);
            await context.SaveChangesAsync();

            TempData["Success"] =
                "Internship deleted successfully.";

            return RedirectToAction(nameof(MyInternships));
        }

        private void ValidateInternshipDates(
            InternshipFormViewModel model)
        {
            if (model.ApplicationDeadline < DateTime.Today)
            {
                ModelState.AddModelError(
                    "ApplicationDeadline",
                    "Application deadline cannot be in the past.");
            }

            if (model.StartDate <= model.ApplicationDeadline)
            {
                ModelState.AddModelError(
                    "StartDate",
                    "Start date must be after the application deadline.");
            }

            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "End date must be after the start date.");
            }
        }
    }
}