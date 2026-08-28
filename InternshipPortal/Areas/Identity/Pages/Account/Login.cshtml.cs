using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InternshipPortal.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ILogger<LoginModel> logger;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            ILogger<LoginModel> logger)
        {
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }
            = new InputModel();

        public IList<AuthenticationScheme>? ExternalLogins
        {
            get;
            set;
        }

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email address is required.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            [Display(Name = "Email Address")]
            public string Email { get; set; }
                = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
                = string.Empty;

            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(
            string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                Response.Redirect(
                    Url.Action(
                        "Index",
                        "Dashboard") ??
                    "/Dashboard");

                return;
            }

            if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                ModelState.AddModelError(
                    string.Empty,
                    ErrorMessage);
            }

            ReturnUrl =
                string.IsNullOrWhiteSpace(returnUrl)
                    ? Url.Action(
                        "Index",
                        "Dashboard")
                    : returnUrl;

            await HttpContext.SignOutAsync(
                IdentityConstants.ExternalScheme);

            ExternalLogins =
                (await signInManager
                    .GetExternalAuthenticationSchemesAsync())
                    .ToList();
        }

        public async Task<IActionResult> OnPostAsync(
            string? returnUrl = null)
        {
            ReturnUrl =
                string.IsNullOrWhiteSpace(returnUrl)
                    ? Url.Action(
                        "Index",
                        "Dashboard")
                    : returnUrl;

            ExternalLogins =
                (await signInManager
                    .GetExternalAuthenticationSchemesAsync())
                    .ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result =
                await signInManager.PasswordSignInAsync(
                    Input.Email.Trim(),
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: true);

            if (result.Succeeded)
            {
                logger.LogInformation(
                    "User logged in successfully.");

                if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
                    Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);
                }

                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToPage(
                    "./LoginWith2fa",
                    new
                    {
                        ReturnUrl,
                        RememberMe = Input.RememberMe
                    });
            }

            if (result.IsLockedOut)
            {
                logger.LogWarning(
                    "User account is locked.");

                ModelState.AddModelError(
                    string.Empty,
                    "Your account is locked. Please contact the administrator.");

                return Page();
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This account is not allowed to sign in.");

                return Page();
            }

            ModelState.AddModelError(
                string.Empty,
                "Invalid email address or password.");

            return Page();
        }
    }
}