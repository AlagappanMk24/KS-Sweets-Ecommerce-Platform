// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KS_Sweets.Domain.Entities.Identity;

namespace KS_Sweets.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger) : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly ILogger<LoginModel> _logger = logger;

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear existing external cookie
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // ✅ Load external login providers (Google)
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
                return Page();

            var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);

            if (user != null)
            {
                var passwordValid = await _signInManager.UserManager.CheckPasswordAsync(user, Input.Password);

                if (passwordValid)
                {
                    // 🚀 Go to OTP screen (user NOT logged in yet)
                    return RedirectToPage("./LoginWithEmailOtp", new
                    {
                        email = Input.Email,
                        returnUrl = returnUrl
                    });
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        // ✅ New method for external (Google) login
        public IActionResult OnPostExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider (e.g. Google)
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }
    }
}
