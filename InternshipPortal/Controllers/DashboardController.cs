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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser =
                await userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            var roles =
                await userManager.GetRolesAsync(currentUser);

            if (!roles.Any())
            {
                return RedirectToAction(
                    "CompleteSetup",
                    "Account");
            }

            if (User.IsInRole("Admin"))
            {
                return await CreateAdminDashboardAsync(
                    currentUser);
            }

            if (User.IsInRole("Student"))
            {
                return await CreateStudentDashboardAsync(
                    currentUser);
            }

            if (User.IsInRole("Company"))
            {
                return await CreateCompanyDashboardAsync(
                    currentUser);
            }

            return Forbid();
        }

        private async Task<IActionResult>
            CreateAdminDashboardAsync(
                IdentityUser currentUser)
        {
            var students =
                await userManager.GetUsersInRoleAsync(
                    "Student");

            var companies =
                await userManager.GetUsersInRoleAsync(
                    "Company");

            var model = new DashboardViewModel
            {
                AccountType = "Administrator",

                DisplayName =
                    currentUser.Email ??
                    currentUser.UserName ??
                    "Administrator",

                ProfileCompleted = true,

                TotalStudents = students.Count,

                TotalCompanies = companies.Count,

                TotalInternships =
                    await context.Internships.CountAsync(),

                ApprovedInternships =
                    await context.Internships
                        .CountAsync(internship =>
                            internship.IsApproved),

                TotalApplications =
                    await context.InternshipApplications
                        .CountAsync(),

                PendingApplications =
                    await context.InternshipApplications
                        .CountAsync(application =>
                            application.Status ==
                                ApplicationStatus.Pending ||
                            application.Status ==
                                ApplicationStatus.UnderReview ||
                            application.Status ==
                                ApplicationStatus.InterviewScheduled),

                AcceptedApplications =
                    await context.InternshipApplications
                        .CountAsync(application =>
                            application.Status ==
                                ApplicationStatus.Accepted),

                RecentInternships =
                    await context.Internships
                        .Include(internship =>
                            internship.Company)
                        .OrderByDescending(internship =>
                            internship.CreatedAt)
                        .Take(5)
                        .ToListAsync(),

                RecentApplications =
                    await context.InternshipApplications
                        .Include(application =>
                            application.Student)
                        .Include(application =>
                            application.Internship)
                        .ThenInclude(internship =>
                            internship.Company)
                        .OrderByDescending(application =>
                            application.AppliedAt)
                        .Take(5)
                        .ToListAsync()
            };

            return View("Index", model);
        }

        private async Task<IActionResult>
            CreateStudentDashboardAsync(
                IdentityUser currentUser)
        {
            var student = await context.Students
                .FirstOrDefaultAsync(student =>
                    student.UserId == currentUser.Id);

            if (student == null)
            {
                var incompleteModel =
                    new DashboardViewModel
                    {
                        AccountType = "Student",

                        DisplayName =
                            currentUser.Email ??
                            currentUser.UserName ??
                            "Student",

                        ProfileCompleted = false
                    };

                return View("Index", incompleteModel);
            }

            var currentDate = DateTime.Now;

            var recentApplications =
                await context.InternshipApplications
                    .Include(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                    .Include(application =>
                        application.Interviews)
                    .Where(application =>
                        application.StudentId ==
                            student.Id)
                    .OrderByDescending(application =>
                        application.AppliedAt)
                    .Take(5)
                    .ToListAsync();

            var upcomingInterviews =
                await context.Interviews
                    .Include(interview =>
                        interview.InternshipApplication)
                    .ThenInclude(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                    .Where(interview =>
                        interview.InternshipApplication
                            .StudentId == student.Id &&
                        interview.ScheduledAt >
                            currentDate &&
                        (interview.Status ==
                            InterviewStatus.Pending ||
                         interview.Status ==
                            InterviewStatus.AcceptedByStudent))
                    .OrderBy(interview =>
                        interview.ScheduledAt)
                    .Take(5)
                    .ToListAsync();

            var recentSavedInternships =
                await context.SavedInternships
                    .Include(saved =>
                        saved.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                    .Where(saved =>
                        saved.StudentId == student.Id)
                    .OrderByDescending(saved =>
                        saved.SavedAt)
                    .Take(5)
                    .ToListAsync();

            var model = new DashboardViewModel
            {
                AccountType = "Student",
                DisplayName = student.FullName,
                ProfileCompleted = true,
                StudentId = student.Id,

                StudentApplications =
                    await context.InternshipApplications
                        .CountAsync(application =>
                            application.StudentId ==
                                student.Id),

                StudentPendingApplications =
                    await context.InternshipApplications
                        .CountAsync(application =>
                            application.StudentId ==
                                student.Id &&
                            (application.Status ==
                                ApplicationStatus.Pending ||
                             application.Status ==
                                ApplicationStatus.UnderReview ||
                             application.Status ==
                                ApplicationStatus.InterviewScheduled)),

                StudentAcceptedApplications =
                    await context.InternshipApplications
                        .CountAsync(application =>
                            application.StudentId ==
                                student.Id &&
                            application.Status ==
                                ApplicationStatus.Accepted),

                StudentSavedInternships =
                    await context.SavedInternships
                        .CountAsync(saved =>
                            saved.StudentId ==
                                student.Id),

                StudentUpcomingInterviews =
                    await context.Interviews
                        .CountAsync(interview =>
                            interview.InternshipApplication
                                .StudentId == student.Id &&
                            interview.ScheduledAt >
                                currentDate &&
                            (interview.Status ==
                                InterviewStatus.Pending ||
                             interview.Status ==
                                InterviewStatus.AcceptedByStudent)),

                ApprovedInternships =
                    await context.Internships
                        .CountAsync(internship =>
                            internship.IsApproved &&
                            internship.IsActive &&
                            internship.ApplicationDeadline >=
                                DateTime.Today),

                RecentInternships =
                    await context.Internships
                        .Include(internship =>
                            internship.Company)
                        .Where(internship =>
                            internship.IsApproved &&
                            internship.IsActive &&
                            internship.ApplicationDeadline >=
                                DateTime.Today)
                        .OrderByDescending(internship =>
                            internship.CreatedAt)
                        .Take(5)
                        .ToListAsync(),

                RecentApplications =
                    recentApplications,

                UpcomingInterviews =
                    upcomingInterviews,

                RecentSavedInternships =
                    recentSavedInternships
            };

            return View("Index", model);
        }

        private async Task<IActionResult>
            CreateCompanyDashboardAsync(
                IdentityUser currentUser)
        {
            var company = await context.Companies
                .FirstOrDefaultAsync(company =>
                    company.UserId == currentUser.Id);

            if (company == null)
            {
                var incompleteModel =
                    new DashboardViewModel
                    {
                        AccountType = "Company",

                        DisplayName =
                            currentUser.Email ??
                            currentUser.UserName ??
                            "Company",

                        ProfileCompleted = false
                    };

                return View("Index", incompleteModel);
            }

            var currentDate = DateTime.Now;

            var recentApplications =
                await context.InternshipApplications
                    .Include(application =>
                        application.Student)
                    .Include(application =>
                        application.Internship)
                    .Where(application =>
                        application.Internship.CompanyId ==
                            company.Id)
                    .OrderByDescending(application =>
                        application.AppliedAt)
                    .Take(5)
                    .ToListAsync();

            var upcomingInterviews =
                await context.Interviews
                    .Include(interview =>
                        interview.InternshipApplication)
                    .ThenInclude(application =>
                        application.Student)
                    .Include(interview =>
                        interview.InternshipApplication)
                    .ThenInclude(application =>
                        application.Internship)
                    .Where(interview =>
                        interview.InternshipApplication
                            .Internship.CompanyId ==
                                company.Id &&
                        interview.ScheduledAt >
                            currentDate &&
                        interview.Status !=
                            InterviewStatus.Cancelled &&
                        interview.Status !=
                            InterviewStatus.DeclinedByStudent &&
                        interview.Status !=
                            InterviewStatus.Completed)
                    .OrderBy(interview =>
                        interview.ScheduledAt)
                    .Take(5)
                    .ToListAsync();

            var model = new DashboardViewModel
            {
                AccountType = "Company",
                DisplayName = company.Name,
                ProfileCompleted = true,
                CompanyId = company.Id,

                CompanyInternships =
                    await context.Internships
                        .CountAsync(internship =>
                            internship.CompanyId ==
                                company.Id),

                CompanyApprovedInternships =
                    await context.Internships
                        .CountAsync(internship =>
                            internship.CompanyId ==
                                company.Id &&
                            internship.IsApproved),

                CompanyPendingInternships =
                    await context.Internships
                        .CountAsync(internship =>
                            internship.CompanyId ==
                                company.Id &&
                            !internship.IsApproved),

                CompanyApplicants =
                    await context.InternshipApplications
                        .CountAsync(application =>
                            application.Internship.CompanyId ==
                                company.Id),

                CompanyUpcomingInterviews =
                    await context.Interviews
                        .CountAsync(interview =>
                            interview.InternshipApplication
                                .Internship.CompanyId ==
                                    company.Id &&
                            interview.ScheduledAt >
                                currentDate &&
                            interview.Status !=
                                InterviewStatus.Cancelled &&
                            interview.Status !=
                                InterviewStatus.DeclinedByStudent &&
                            interview.Status !=
                                InterviewStatus.Completed),

                RecentInternships =
                    await context.Internships
                        .Include(internship =>
                            internship.Company)
                        .Where(internship =>
                            internship.CompanyId ==
                                company.Id)
                        .OrderByDescending(internship =>
                            internship.CreatedAt)
                        .Take(5)
                        .ToListAsync(),

                RecentApplications =
                    recentApplications,

                UpcomingInterviews =
                    upcomingInterviews
            };

            return View("Index", model);
        }
    }
}