using InternshipPortal.Data;
using InternshipPortal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserManagementController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? role)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                usersQuery = usersQuery.Where(user =>
                    user.Email != null &&
                    user.Email.Contains(search));
            }

            var users = await usersQuery
                .OrderByDescending(user => user.Id)
                .ToListAsync();

            var students = await _context.Students
                .ToDictionaryAsync(
                    student => student.UserId,
                    student => student.FullName);

            var companies = await _context.Companies
                .ToDictionaryAsync(
                    company => company.UserId,
                    company => company.Name);

            var viewModels = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "No Role";

                if (!string.IsNullOrWhiteSpace(role) &&
                    !userRole.Equals(
                        role,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var displayName = user.Email ?? "Unknown User";

                if (userRole == "Student" &&
                    students.TryGetValue(user.Id, out var studentName))
                {
                    displayName = studentName;
                }
                else if (userRole == "Company" &&
                         companies.TryGetValue(user.Id, out var companyName))
                {
                    displayName = companyName;
                }
                else if (userRole == "Admin")
                {
                    displayName = "System Administrator";
                }

                var isLocked =
                    user.LockoutEnd.HasValue &&
                    user.LockoutEnd.Value > DateTimeOffset.UtcNow;

                viewModels.Add(new UserManagementViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    Role = userRole,
                    DisplayName = displayName,
                    IsLocked = isLocked,
                    LockoutEnd = user.LockoutEnd?.DateTime
                });
            }

            ViewBag.Search = search;
            ViewBag.SelectedRole = role;

            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (id == currentUserId)
            {
                TempData["ErrorMessage"] =
                    "You cannot lock your own administrator account.";

                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            await _userManager.SetLockoutEnabledAsync(user, true);

            var result = await _userManager.SetLockoutEndDateAsync(
                user,
                DateTimeOffset.MaxValue);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] =
                    "The account could not be locked.";
            }
            else
            {
                TempData["SuccessMessage"] =
                    "The account has been locked successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.SetLockoutEndDateAsync(
                user,
                null);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] =
                    "The account could not be unlocked.";
            }
            else
            {
                await _userManager.ResetAccessFailedCountAsync(user);

                TempData["SuccessMessage"] =
                    "The account has been activated successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}