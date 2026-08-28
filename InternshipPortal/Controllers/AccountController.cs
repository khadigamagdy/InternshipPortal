using InternshipPortal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InternshipPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (model.Role != "Student" &&
                model.Role != "Company")
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Please select a valid account type.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!await roleManager.RoleExistsAsync(model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "The selected account type is unavailable.");

                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(
                user,
                model.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            var roleResult = await userManager.AddToRoleAsync(
                user,
                model.Role);

            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                ModelState.AddModelError(
                    string.Empty,
                    "The account could not be assigned to a role.");

                return View(model);
            }

            await signInManager.SignInAsync(
                user,
                isPersistent: false);

            if (model.Role == "Student")
            {
                return RedirectToAction(
                    "Profile",
                    "Student");
            }

            return RedirectToAction(
                "Profile",
                "Company");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CompleteSetup()
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var roles = await userManager.GetRolesAsync(user);

            if (roles.Any())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            return View(new CompleteAccountSetupViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteSetup(
            CompleteAccountSetupViewModel model)
        {
            if (model.Role != "Student" &&
                model.Role != "Company")
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Please select Student or Company.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var currentRoles =
                await userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            if (!await roleManager.RoleExistsAsync(model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "The selected account type is unavailable.");

                return View(model);
            }

            var roleResult = await userManager.AddToRoleAsync(
                user,
                model.Role);

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            await signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] =
                "Your account type has been saved successfully.";

            if (model.Role == "Student")
            {
                return RedirectToAction(
                    "Profile",
                    "Student");
            }

            return RedirectToAction(
                "Profile",
                "Company");
        }
    }
}