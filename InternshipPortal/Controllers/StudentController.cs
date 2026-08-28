using InternshipPortal.Data;
using InternshipPortal.Models;
using InternshipPortal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public StudentController(
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

            var student = await context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return View(new StudentProfileViewModel());
            }

            var model = new StudentProfileViewModel
            {
                Id = student.Id,
                FullName = student.FullName,
                University = student.University,
                Faculty = student.Faculty,
                Specialization = student.Specialization,
                GraduationYear = student.GraduationYear,
                CurrentCVPath = student.CVPath
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(
            StudentProfileViewModel model)
        {
            var userId = userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var student = await context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (model.CVFile != null)
            {
                var extension =
                    Path.GetExtension(model.CVFile.FileName)
                        .ToLower();

                if (extension != ".pdf")
                {
                    ModelState.AddModelError(
                        "CVFile",
                        "Only PDF files are allowed.");
                }

                if (model.CVFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "CVFile",
                        "The CV must not exceed 5 MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                if (student != null)
                {
                    model.CurrentCVPath = student.CVPath;
                }

                return View(model);
            }

            string? cvPath = student?.CVPath;

            if (model.CVFile != null)
            {
                var uploadsFolder = Path.Combine(
                    webHostEnvironment.WebRootPath,
                    "uploads",
                    "cvs");

                Directory.CreateDirectory(uploadsFolder);

                var fileName =
                    $"{Guid.NewGuid()}{Path.GetExtension(model.CVFile.FileName)}";

                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName);

                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await model.CVFile.CopyToAsync(stream);
                }

                cvPath = $"/uploads/cvs/{fileName}";
            }

            if (student == null)
            {
                student = new Student
                {
                    FullName = model.FullName,
                    University = model.University,
                    Faculty = model.Faculty,
                    Specialization = model.Specialization,
                    GraduationYear = model.GraduationYear,
                    CVPath = cvPath,
                    UserId = userId
                };

                context.Students.Add(student);
            }
            else
            {
                student.FullName = model.FullName;
                student.University = model.University;
                student.Faculty = model.Faculty;
                student.Specialization = model.Specialization;
                student.GraduationYear = model.GraduationYear;
                student.CVPath = cvPath;
            }

            await context.SaveChangesAsync();

            TempData["Success"] =
                "Your profile has been saved successfully.";

            return RedirectToAction(
                "Index",
                "Dashboard");
        }
    }
}