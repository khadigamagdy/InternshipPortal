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
    public class StudentPreferenceController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public StudentPreferenceController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = userManager.GetUserId(User);

            var student = await context.Students
                .Include(student => student.Preference)
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

            var model = new StudentPreferenceViewModel();

            if (student.Preference != null)
            {
                model.Skills =
                    student.Preference.Skills;

                model.CareerInterests =
                    student.Preference.CareerInterests;

                model.PreferredLocation =
                    student.Preference.PreferredLocation;

                model.PreferredWorkMode =
                    student.Preference.PreferredWorkMode;

                model.MinimumSalary =
                    student.Preference.MinimumSalary;

                model.AcceptUnpaidInternships =
                    student.Preference.AcceptUnpaidInternships;

                model.AcceptRemoteInternships =
                    student.Preference.AcceptRemoteInternships;

                model.MaximumWeeklyHours =
                    student.Preference.MaximumWeeklyHours;
            }

            ViewBag.StudentName = student.FullName;
            ViewBag.HasPreferences =
                student.Preference != null;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            StudentPreferenceViewModel model)
        {
            var userId = userManager.GetUserId(User);

            var student = await context.Students
                .Include(student => student.Preference)
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

            ViewBag.StudentName = student.FullName;
            ViewBag.HasPreferences =
                student.Preference != null;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (student.Preference == null)
            {
                var preference = new StudentPreference
                {
                    StudentId = student.Id,

                    Skills =
                        model.Skills.Trim(),

                    CareerInterests =
                        model.CareerInterests?.Trim(),

                    PreferredLocation =
                        model.PreferredLocation?.Trim(),

                    PreferredWorkMode =
                        model.PreferredWorkMode,

                    MinimumSalary =
                        model.MinimumSalary,

                    AcceptUnpaidInternships =
                        model.AcceptUnpaidInternships,

                    AcceptRemoteInternships =
                        model.AcceptRemoteInternships,

                    MaximumWeeklyHours =
                        model.MaximumWeeklyHours,

                    UpdatedAt =
                        DateTime.Now
                };

                context.StudentPreferences.Add(preference);
            }
            else
            {
                student.Preference.Skills =
                    model.Skills.Trim();

                student.Preference.CareerInterests =
                    model.CareerInterests?.Trim();

                student.Preference.PreferredLocation =
                    model.PreferredLocation?.Trim();

                student.Preference.PreferredWorkMode =
                    model.PreferredWorkMode;

                student.Preference.MinimumSalary =
                    model.MinimumSalary;

                student.Preference.AcceptUnpaidInternships =
                    model.AcceptUnpaidInternships;

                student.Preference.AcceptRemoteInternships =
                    model.AcceptRemoteInternships;

                student.Preference.MaximumWeeklyHours =
                    model.MaximumWeeklyHours;

                student.Preference.UpdatedAt =
                    DateTime.Now;
            }

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Preferences saved. Your matches have been updated.";

            return RedirectToAction(
                "Index",
                "Matching");
        }
    }
}