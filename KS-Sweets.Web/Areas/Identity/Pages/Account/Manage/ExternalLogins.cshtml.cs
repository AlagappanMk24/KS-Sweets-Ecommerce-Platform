using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using KS_Sweets.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KS_Sweets.Web.Areas.Identity.Pages.Account.Manage
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }
        public IActionResult OnGet() => RedirectToPage("./Login");
        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        // ✅ Callback بعد تسجيل الدخول من Google
        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // ✅ محاولة تسجيل الدخول لو المستخدم موجود بالفعل
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: true,
                bypassTwoFactor: true
            );

            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);

                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                // ✅ تحميل صفحة الترحيب بدل التوجيه الفوري
                ProviderDisplayName = info.LoginProvider;
                ReturnUrl = returnUrl;

                // ✅ إرسال الدور إلى الصفحة لو حابب تستخدمه هناك
                ViewData["IsAdmin"] = await _userManager.IsInRoleAsync(user, "Admin");

                return Page();
            }

            if (result.IsLockedOut)
                return RedirectToPage("./Lockout");

            // ✅ المستخدم أول مرة يسجل بجوجل
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        foreach (var error in createResult.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                    }

                    await _userManager.AddLoginAsync(user, info);
                    _logger.LogInformation("New user created using {Name} provider.", info.LoginProvider);
                }

                // ✅ تسجيل الدخول التلقائي بعد الإنشاء
                await _signInManager.SignInAsync(user, isPersistent: true);
                _logger.LogInformation("User {Email} logged in with Google.", email);

                // ✅ تحميل صفحة الترحيب (ExternalLogin) بدل التوجيه المباشر
                ProviderDisplayName = info.LoginProvider;
                ReturnUrl = returnUrl;

                ViewData["IsAdmin"] = await _userManager.IsInRoleAsync(user, "Admin");

                return Page();
            }

            ErrorMessage = "Google account did not provide an email address.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        // ✅ في حالة تأكيد الإيميل (اختياري)
        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        ProviderDisplayName = info.LoginProvider;
                        ReturnUrl = returnUrl;
                        return Page();
                    }
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("The default UI requires a user store with email support.");
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}