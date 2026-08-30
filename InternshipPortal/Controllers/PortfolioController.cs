using System.Text;
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
    public class PortfolioController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public PortfolioController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            this.context = context;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                TempData["Error"] =
                    "Please complete your student profile first.";

                return RedirectToAction("Profile", "Student");
            }

            var portfolio = await context.StudentPortfolios
                .Include(portfolio => portfolio.Projects)
                .FirstOrDefaultAsync(portfolio =>
                    portfolio.StudentId == student.Id);

            if (portfolio == null)
            {
                portfolio = new StudentPortfolio
                {
                    StudentId = student.Id,
                    Headline =
                        $"{student.Specialization} Student",
                    Bio =
                        $"I am {student.FullName}, a student at " +
                        $"{student.University}.",
                    SkillsSummary = student.Specialization,
                    PortfolioSlug =
                        await GenerateUniqueSlugAsync(
                            student.FullName),
                    IsPublic = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                context.StudentPortfolios.Add(portfolio);
                await context.SaveChangesAsync();
            }

            var model = await BuildDetailsViewModelAsync(
                student,
                portfolio);

            return View(model);
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                TempData["Error"] =
                    "Please complete your student profile first.";

                return RedirectToAction("Profile", "Student");
            }

            var portfolio = await context.StudentPortfolios
                .FirstOrDefaultAsync(portfolio =>
                    portfolio.StudentId == student.Id);

            if (portfolio == null)
            {
                portfolio = new StudentPortfolio
                {
                    StudentId = student.Id,
                    Headline =
                        $"{student.Specialization} Student",
                    Bio =
                        $"I am {student.FullName}, a student at " +
                        $"{student.University}.",
                    SkillsSummary = student.Specialization,
                    PortfolioSlug =
                        await GenerateUniqueSlugAsync(
                            student.FullName),
                    IsPublic = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                context.StudentPortfolios.Add(portfolio);
                await context.SaveChangesAsync();
            }

            var model = new StudentPortfolioEditViewModel
            {
                Headline = portfolio.Headline,
                Bio = portfolio.Bio,
                SkillsSummary = portfolio.SkillsSummary,
                GitHubUrl = portfolio.GitHubUrl,
                LinkedInUrl = portfolio.LinkedInUrl,
                PersonalWebsiteUrl =
                    portfolio.PersonalWebsiteUrl,
                IsPublic = portfolio.IsPublic
            };

            return View(model);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            StudentPortfolioEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return NotFound();
            }

            var portfolio = await context.StudentPortfolios
                .FirstOrDefaultAsync(portfolio =>
                    portfolio.StudentId == student.Id);

            if (portfolio == null)
            {
                portfolio = new StudentPortfolio
                {
                    StudentId = student.Id,
                    PortfolioSlug =
                        await GenerateUniqueSlugAsync(
                            student.FullName),
                    CreatedAt = DateTime.Now
                };

                context.StudentPortfolios.Add(portfolio);
            }

            portfolio.Headline = model.Headline.Trim();
            portfolio.Bio = model.Bio.Trim();
            portfolio.SkillsSummary =
                model.SkillsSummary?.Trim();
            portfolio.GitHubUrl = model.GitHubUrl?.Trim();
            portfolio.LinkedInUrl =
                model.LinkedInUrl?.Trim();
            portfolio.PersonalWebsiteUrl =
                model.PersonalWebsiteUrl?.Trim();
            portfolio.IsPublic = model.IsPublic;
            portfolio.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Your portfolio has been updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> AddProject()
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                TempData["Error"] =
                    "Please complete your student profile first.";

                return RedirectToAction("Profile", "Student");
            }

            return View(new PortfolioProjectViewModel());
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProject(
            PortfolioProjectViewModel model)
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return NotFound();
            }

            var portfolio = await context.StudentPortfolios
                .FirstOrDefaultAsync(portfolio =>
                    portfolio.StudentId == student.Id);

            if (portfolio == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please create your portfolio first.");
            }

            ValidateProjectImage(model.ImageFile);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? imagePath = null;

            if (model.ImageFile != null)
            {
                imagePath = await SaveProjectImageAsync(
                    model.ImageFile);
            }

            var project = new PortfolioProject
            {
                Title = model.Title.Trim(),
                Description = model.Description.Trim(),
                Technologies = model.Technologies.Trim(),
                ProjectUrl = model.ProjectUrl?.Trim(),
                RepositoryUrl = model.RepositoryUrl?.Trim(),
                ImagePath = imagePath,
                IsFeatured = model.IsFeatured,
                CreatedAt = DateTime.Now,
                StudentPortfolioId = portfolio!.Id
            };

            context.PortfolioProjects.Add(project);

            portfolio.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Project added to your portfolio.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> EditProject(int id)
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return NotFound();
            }

            var project = await context.PortfolioProjects
                .Include(project =>
                    project.StudentPortfolio)
                .FirstOrDefaultAsync(project =>
                    project.Id == id &&
                    project.StudentPortfolio.StudentId ==
                        student.Id);

            if (project == null)
            {
                return NotFound();
            }

            var model = new PortfolioProjectViewModel
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                Technologies = project.Technologies,
                ProjectUrl = project.ProjectUrl,
                RepositoryUrl = project.RepositoryUrl,
                CurrentImagePath = project.ImagePath,
                IsFeatured = project.IsFeatured
            };

            return View(model);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProject(
            PortfolioProjectViewModel model)
        {
            if (!model.Id.HasValue)
            {
                return NotFound();
            }

            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return NotFound();
            }

            var project = await context.PortfolioProjects
                .Include(project =>
                    project.StudentPortfolio)
                .FirstOrDefaultAsync(project =>
                    project.Id == model.Id.Value &&
                    project.StudentPortfolio.StudentId ==
                        student.Id);

            if (project == null)
            {
                return NotFound();
            }

            model.CurrentImagePath = project.ImagePath;

            ValidateProjectImage(model.ImageFile);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.ImageFile != null)
            {
                DeleteProjectImage(project.ImagePath);

                project.ImagePath =
                    await SaveProjectImageAsync(
                        model.ImageFile);
            }

            project.Title = model.Title.Trim();
            project.Description =
                model.Description.Trim();
            project.Technologies =
                model.Technologies.Trim();
            project.ProjectUrl =
                model.ProjectUrl?.Trim();
            project.RepositoryUrl =
                model.RepositoryUrl?.Trim();
            project.IsFeatured = model.IsFeatured;

            project.StudentPortfolio.UpdatedAt =
                DateTime.Now;

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Project updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var student = await GetCurrentStudentAsync();

            if (student == null)
            {
                return NotFound();
            }

            var project = await context.PortfolioProjects
                .Include(project =>
                    project.StudentPortfolio)
                .FirstOrDefaultAsync(project =>
                    project.Id == id &&
                    project.StudentPortfolio.StudentId ==
                        student.Id);

            if (project == null)
            {
                return NotFound();
            }

            DeleteProjectImage(project.ImagePath);

            project.StudentPortfolio.UpdatedAt =
                DateTime.Now;

            context.PortfolioProjects.Remove(project);

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Project removed from your portfolio.";

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Public(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }

            var portfolio = await context.StudentPortfolios
                .Include(portfolio => portfolio.Student)
                .Include(portfolio => portfolio.Projects)
                .FirstOrDefaultAsync(portfolio =>
                    portfolio.PortfolioSlug == slug &&
                    portfolio.IsPublic);

            if (portfolio == null)
            {
                return NotFound();
            }

            var model = await BuildDetailsViewModelAsync(
                portfolio.Student,
                portfolio);

            return View(model);
        }

        private async Task<Student?>
            GetCurrentStudentAsync()
        {
            var userId = userManager.GetUserId(User);

            return await context.Students
                .FirstOrDefaultAsync(student =>
                    student.UserId == userId);
        }

        private async Task<StudentPortfolioDetailsViewModel>
            BuildDetailsViewModelAsync(
                Student student,
                StudentPortfolio portfolio)
        {
            var completedTrainings =
                await context.InternshipApplications
                    .Include(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                    .Include(application =>
                        application.Evaluation)
                    .Include(application =>
                        application.TrainingEnrollment)
                    .ThenInclude(enrollment =>
                        enrollment!.HourEntries)
                    .Where(application =>
                        application.StudentId == student.Id &&
                        application.Status ==
                            ApplicationStatus.Completed)
                    .OrderByDescending(application =>
                        application.ReviewedAt)
                    .ToListAsync();

            var ratings = completedTrainings
                .Where(application =>
                    application.Evaluation != null)
                .Select(application =>
                    application.Evaluation!.Rating)
                .ToList();

            var approvedHours = completedTrainings
                .Where(application =>
                    application.TrainingEnrollment != null)
                .SelectMany(application =>
                    application.TrainingEnrollment!
                        .HourEntries)
                .Where(entry =>
                    entry.Status ==
                        TrainingHourStatus.Approved)
                .Sum(entry => entry.Hours);

            return new StudentPortfolioDetailsViewModel
            {
                Student = student,
                Portfolio = portfolio,
                Projects = portfolio.Projects
                    .OrderByDescending(project =>
                        project.IsFeatured)
                    .ThenByDescending(project =>
                        project.CreatedAt)
                    .ToList(),
                CompletedTrainings = completedTrainings,
                CompletedTrainingsCount =
                    completedTrainings.Count,
                ApprovedTrainingHours =
                    Convert.ToInt32(approvedHours),
                ProjectsCount = portfolio.Projects.Count,
                EvaluationsCount = ratings.Count,
                AverageRating = ratings.Any()
                    ? ratings.Average()
                    : 0
            };
        }

        private void ValidateProjectImage(
            IFormFile? imageFile)
        {
            if (imageFile == null)
            {
                return;
            }

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension = Path
                .GetExtension(imageFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "ImageFile",
                    "Only JPG, PNG and WEBP images are allowed.");
            }

            if (imageFile.Length > 4 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "ImageFile",
                    "The project image must not exceed 4 MB.");
            }
        }

        private async Task<string> SaveProjectImageAsync(
            IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(
                webHostEnvironment.WebRootPath,
                "uploads",
                "portfolio-projects");

            Directory.CreateDirectory(uploadsFolder);

            var extension = Path
                .GetExtension(imageFile.FileName)
                .ToLowerInvariant();

            var fileName = $"{Guid.NewGuid()}{extension}";

            var physicalPath = Path.Combine(
                uploadsFolder,
                fileName);

            await using var stream = new FileStream(
                physicalPath,
                FileMode.Create);

            await imageFile.CopyToAsync(stream);

            return $"/uploads/portfolio-projects/{fileName}";
        }

        private void DeleteProjectImage(
            string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            var relativePath = imagePath
                .TrimStart('/')
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar);

            var physicalPath = Path.Combine(
                webHostEnvironment.WebRootPath,
                relativePath);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }

        private async Task<string>
            GenerateUniqueSlugAsync(string fullName)
        {
            var slug = CreateSlug(fullName);

            if (string.IsNullOrWhiteSpace(slug))
            {
                slug = "student";
            }

            var originalSlug = slug;
            var number = 1;

            while (await context.StudentPortfolios
                .AnyAsync(portfolio =>
                    portfolio.PortfolioSlug == slug))
            {
                slug = $"{originalSlug}-{number}";
                number++;
            }

            return slug;
        }

        private static string CreateSlug(string value)
        {
            var builder = new StringBuilder();

            foreach (var character in value
                .Trim()
                .ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character);
                }
                else if (char.IsWhiteSpace(character) ||
                         character == '-' ||
                         character == '_')
                {
                    if (builder.Length > 0 &&
                        builder[^1] != '-')
                    {
                        builder.Append('-');
                    }
                }
            }

            return builder
                .ToString()
                .Trim('-');
        }
    }
}