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

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? location,
            WorkMode? workMode)
        {
            var today = DateTime.Today;

            var internships = context.Internships
                .AsNoTracking()
                .Include(internship =>
                    internship.Company)
                .Where(internship =>
                    internship.IsApproved &&
                    internship.IsActive &&
                    internship.ApplicationDeadline >= today &&
                    internship.AvailablePositions > 0)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchText =
                    search.Trim();

                internships = internships.Where(internship =>
                    internship.Title.Contains(searchText) ||
                    internship.Description.Contains(searchText) ||
                    internship.RequiredSkills.Contains(searchText) ||
                    internship.Company.Name.Contains(searchText));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                var locationText =
                    location.Trim();

                internships = internships.Where(internship =>
                    internship.Location.Contains(locationText));
            }

            if (workMode.HasValue)
            {
                internships = internships.Where(internship =>
                    internship.WorkMode == workMode.Value);
            }

            ViewBag.Search = search;
            ViewBag.Location = location;
            ViewBag.WorkMode = workMode;

            var results = await internships
                .OrderByDescending(internship =>
                    internship.CreatedAt)
                .ToListAsync();

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var internship = await context.Internships
                .AsNoTracking()
                .Include(internship =>
                    internship.Company)
                .Include(internship =>
                    internship.Applications)
                .FirstOrDefaultAsync(internship =>
                    internship.Id == id);

            if (internship == null)
            {
                return NotFound();
            }

            var userId =
                userManager.GetUserId(User);

            var isAdmin =
                User.IsInRole("Admin");

            var isOwner =
                User.IsInRole("Company") &&
                internship.Company.UserId == userId;

            if ((!internship.IsApproved ||
                 !internship.IsActive) &&
                !isAdmin &&
                !isOwner)
            {
                return Forbid();
            }

            return View(internship);
        }

        [Authorize(Roles = "Company")]
        [HttpGet]
        public async Task<IActionResult> MyInternships()
        {
            var userId =
                userManager.GetUserId(User);

            var company = await context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(company =>
                    company.UserId == userId);

            if (company == null)
            {
                TempData["Error"] =
                    "Please complete your company profile first.";

                return RedirectToAction(
                    "Profile",
                    "Company");
            }

            var internships = await context.Internships
                .AsNoTracking()
                .Where(internship =>
                    internship.CompanyId == company.Id)
                .OrderByDescending(internship =>
                    internship.CreatedAt)
                .ToListAsync();

            return View(internships);
        }

        [Authorize(Roles = "Company")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId =
                userManager.GetUserId(User);

            var companyExists =
                await context.Companies
                    .AnyAsync(company =>
                        company.UserId == userId);

            if (!companyExists)
            {
                TempData["Error"] =
                    "Please complete your company profile first.";

                return RedirectToAction(
                    "Profile",
                    "Company");
            }

            var model =
                new InternshipFormViewModel
                {
                    StartDate =
                        DateTime.Today.AddDays(14),

                    EndDate =
                        DateTime.Today.AddMonths(2),

                    ApplicationDeadline =
                        DateTime.Today.AddDays(10),

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
            ValidateInternshipPayment(model);

            var userId =
                userManager.GetUserId(User);

            var company = await context.Companies
                .FirstOrDefaultAsync(company =>
                    company.UserId == userId);

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

            var internship =
                new Internship
                {
                    Title =
                        model.Title.Trim(),

                    Description =
                        model.Description.Trim(),

                    RequiredSkills =
                        model.RequiredSkills.Trim(),

                    Location =
                        model.Location.Trim(),

                    WorkMode =
                        model.WorkMode,

                    StartDate =
                        model.StartDate,

                    EndDate =
                        model.EndDate,

                    ApplicationDeadline =
                        model.ApplicationDeadline,

                    AvailablePositions =
                        model.AvailablePositions,

                    IsPaid =
                        model.IsPaid,

                    Salary =
                        model.Salary,

                    IsApproved =
                        false,

                    IsActive =
                        true,

                    CreatedAt =
                        DateTime.Now,

                    CompanyId =
                        company.Id
                };

            context.Internships.Add(internship);

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Internship created and sent for admin approval.";

            return RedirectToAction(
                nameof(MyInternships));
        }

        [Authorize(Roles = "Company")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId =
                userManager.GetUserId(User);

            var internship = await context.Internships
                .AsNoTracking()
                .Include(internship =>
                    internship.Company)
                .FirstOrDefaultAsync(internship =>
                    internship.Id == id &&
                    internship.Company.UserId == userId);

            if (internship == null)
            {
                return NotFound();
            }

            var model =
                new InternshipFormViewModel
                {
                    Id =
                        internship.Id,

                    Title =
                        internship.Title,

                    Description =
                        internship.Description,

                    RequiredSkills =
                        internship.RequiredSkills,

                    Location =
                        internship.Location,

                    WorkMode =
                        internship.WorkMode,

                    StartDate =
                        internship.StartDate,

                    EndDate =
                        internship.EndDate,

                    ApplicationDeadline =
                        internship.ApplicationDeadline,

                    AvailablePositions =
                        internship.AvailablePositions,

                    IsPaid =
                        internship.IsPaid,

                    Salary =
                        internship.Salary
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
            ValidateInternshipPayment(model);

            var userId =
                userManager.GetUserId(User);

            var internship = await context.Internships
                .Include(internship =>
                    internship.Company)
                .FirstOrDefaultAsync(internship =>
                    internship.Id == model.Id &&
                    internship.Company.UserId == userId);

            if (internship == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            internship.Title =
                model.Title.Trim();

            internship.Description =
                model.Description.Trim();

            internship.RequiredSkills =
                model.RequiredSkills.Trim();

            internship.Location =
                model.Location.Trim();

            internship.WorkMode =
                model.WorkMode;

            internship.StartDate =
                model.StartDate;

            internship.EndDate =
                model.EndDate;

            internship.ApplicationDeadline =
                model.ApplicationDeadline;

            internship.AvailablePositions =
                model.AvailablePositions;

            internship.IsPaid =
                model.IsPaid;

            internship.Salary =
                model.Salary;

            internship.IsApproved =
                false;

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Internship updated and sent for approval again.";

            return RedirectToAction(
                nameof(MyInternships));
        }

        [Authorize(Roles = "Company")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId =
                userManager.GetUserId(User);

            var internship = await context.Internships
                .AsNoTracking()
                .Include(internship =>
                    internship.Company)
                .FirstOrDefaultAsync(internship =>
                    internship.Id == id &&
                    internship.Company.UserId == userId);

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
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var userId =
                userManager.GetUserId(User);

            var internship = await context.Internships
                .Include(internship =>
                    internship.Company)
                .FirstOrDefaultAsync(internship =>
                    internship.Id == id &&
                    internship.Company.UserId == userId);

            if (internship == null)
            {
                return NotFound();
            }

            var hasApplications =
                await context.InternshipApplications
                    .AnyAsync(application =>
                        application.InternshipId == id);

            if (hasApplications)
            {
                TempData["Error"] =
                    "This internship cannot be deleted because it has applications.";

                return RedirectToAction(
                    nameof(MyInternships));
            }

            context.Internships.Remove(internship);

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Internship deleted successfully.";

            return RedirectToAction(
                nameof(MyInternships));
        }

        private void ValidateInternshipDates(
            InternshipFormViewModel model)
        {
            if (model.ApplicationDeadline.Date <
                DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.ApplicationDeadline),
                    "Application deadline cannot be in the past.");
            }

            if (model.StartDate.Date <=
                model.ApplicationDeadline.Date)
            {
                ModelState.AddModelError(
                    nameof(model.StartDate),
                    "Start date must be after the application deadline.");
            }

            if (model.EndDate.Date <=
                model.StartDate.Date)
            {
                ModelState.AddModelError(
                    nameof(model.EndDate),
                    "End date must be after the start date.");
            }

            if (model.AvailablePositions <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.AvailablePositions),
                    "At least one available position is required.");
            }
        }

        private void ValidateInternshipPayment(
            InternshipFormViewModel model)
        {
            if (model.IsPaid &&
                (!model.Salary.HasValue ||
                 model.Salary.Value <= 0))
            {
                ModelState.AddModelError(
                    nameof(model.Salary),
                    "A valid salary is required for a paid internship.");
            }

            if (!model.IsPaid)
            {
                model.Salary = null;
            }
        }
    }
}