using InternshipPortal.Data;
using InternshipPortal.Models;
using InternshipPortal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Company")]
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public CompanyController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            this.context = context;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = userManager.GetUserId(User);

            var company = await context.Companies
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (company == null)
            {
                return View(new CompanyProfileViewModel());
            }

            var model = new CompanyProfileViewModel
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description,
                Location = company.Location,
                Website = company.Website,
                CurrentLogoPath = company.LogoPath
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(
            CompanyProfileViewModel model)
        {
            var userId = userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var company = await context.Companies
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (model.LogoFile != null)
            {
                var extension =
                    Path.GetExtension(model.LogoFile.FileName)
                        .ToLower();

                string[] allowedExtensions =
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "LogoFile",
                        "Only JPG, JPEG, PNG and WEBP images are allowed.");
                }

                if (model.LogoFile.Length > 3 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "LogoFile",
                        "The logo must not exceed 3 MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                if (company != null)
                {
                    model.CurrentLogoPath = company.LogoPath;
                }

                return View(model);
            }

            string? logoPath = company?.LogoPath;

            if (model.LogoFile != null)
            {
                var uploadsFolder = Path.Combine(
                    webHostEnvironment.WebRootPath,
                    "uploads",
                    "logos");

                Directory.CreateDirectory(uploadsFolder);

                var fileName =
                    $"{Guid.NewGuid()}{Path.GetExtension(model.LogoFile.FileName)}";

                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName);

                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await model.LogoFile.CopyToAsync(stream);
                }

                logoPath = $"/uploads/logos/{fileName}";
            }

            if (company == null)
            {
                company = new Company
                {
                    Name = model.Name,
                    Description = model.Description,
                    Location = model.Location,
                    Website = model.Website,
                    LogoPath = logoPath,
                    UserId = userId
                };

                context.Companies.Add(company);
            }
            else
            {
                company.Name = model.Name;
                company.Description = model.Description;
                company.Location = model.Location;
                company.Website = model.Website;
                company.LogoPath = logoPath;
            }

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Company profile has been saved successfully.";

            return RedirectToAction(
                "Index",
                "Dashboard");
        }
    }
}