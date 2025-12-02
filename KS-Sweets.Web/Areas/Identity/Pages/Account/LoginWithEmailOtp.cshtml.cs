using KS_Sweets.Application.Contracts.Services;
using KS_Sweets.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace KS_Sweets.Web.Areas.Identity.Pages.Account
{
    public class LoginWithEmailOtpModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<LoginWithEmailOtpModel> logger) : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IEmailService _emailService = emailService;
        private readonly ILogger<LoginWithEmailOtpModel> _logger = logger;

        [BindProperty] public InputModel Input { get; set; }
        public string Email { get; set; }
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required] public string[] OtpDigits { get; set; } = new string[6];
            public string FullOtp => string.Join("", OtpDigits);
        }
        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("./Login");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToPage("./Login");
            }

            Email = user.Email;
            ReturnUrl = returnUrl ?? Url.Content("~/");

            // Generate and send OTP
            var otp = GenerateOtp();
            HttpContext.Session.SetString("EmailOtp", otp);
            HttpContext.Session.SetString("OtpUserEmail", user.Email);
            HttpContext.Session.SetString("OtpExpires", DateTime.UtcNow.AddMinutes(5).ToString("o"));

            await _emailService.SendEmailAsync(user.Email, "Your Login OTP Code",
                $"<div style='font-family:Arial;text-align:center;padding:30px;background:#f8fafc;border-radius:16px'>" +
                $"<h1 style='color:#4f46e5'>Your Code: <strong style='font-size:3rem'>{otp}</strong></h1>" +
                $"<p style='color:#64748b'>Valid for 5 minutes</p></div>");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!ModelState.IsValid) return Page();

            var savedOtp = HttpContext.Session.GetString("EmailOtp");
            var savedEmail = HttpContext.Session.GetString("OtpUserEmail");
            var expiresStr = HttpContext.Session.GetString("OtpExpires");

            if (string.IsNullOrEmpty(savedOtp) || string.IsNullOrEmpty(savedEmail) || string.IsNullOrEmpty(expiresStr))
            {
                ModelState.AddModelError("", "Session expired. Please try again.");
                return Page();
            }

            if (DateTime.Parse(expiresStr) < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "OTP has expired. Please request a new one.");
                return Page();
            }

            if (Input.FullOtp != savedOtp)
            {
                ModelState.AddModelError("", "Invalid verification code.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(savedEmail);
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                HttpContext.Session.Clear(); // Clear OTP
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }

            return RedirectToPage("./Login");
        }

        public async Task<IActionResult> OnPostResendAsync()
        {
            var email = HttpContext.Session.GetString("OtpUserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("./Login");
            }

            // Re-generate and re-send
            var otp = GenerateOtp();
            HttpContext.Session.SetString("EmailOtp", otp);
            HttpContext.Session.SetString("OtpExpires", DateTime.UtcNow.AddMinutes(5).ToString("o"));

            await _emailService.SendEmailAsync(email, "Your New OTP Code",
                $"<div style='font-family:Arial;text-align:center;padding:30px;background:#f8fafc;border-radius:16px'>" +
                $"<h1 style='color:#4f46e5'>New Code: <strong style='font-size:3rem'>{otp}</strong></h1>" +
                $"<p style='color:#64748b'>Valid for 5 minutes</p></div>");

            TempData["ResentMessage"] = "A new OTP has been sent!";
            return RedirectToPage(new { email });
        }

        private static string GenerateOtp() => new Random().Next(100000, 999999).ToString();
    }
}