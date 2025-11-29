using KS_Sweets.Application.Contracts.Services;
using KS_Sweets.Domain.Constants;
using KS_Sweets.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace KS_Sweets.Web.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager; 
        private readonly UserManager<ApplicationUser> _userManager;     
        private readonly IUserStore<ApplicationUser> _userStore;     
        private readonly IUserEmailStore<ApplicationUser> _emailStore; 
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailService _emailService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailService emailService,
            RoleManager<IdentityRole> roleManager
        )
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailService = emailService;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string? Role { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }

            [Required]
            public string Name { get; set; }
            public string? StreetAddress { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
            public string? PostalCode { get; set; }
            public string? PhoneNumber { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            //// 🚀 FIX: Use await to check the role existence asynchronously
            //if (!await _roleManager.RoleExistsAsync(AppRoles.Customer))
            //{
            //    _logger.LogInformation("Creating initial Identity roles...");

            //    // 🚀 FIX: Use await for role creation
            //    await _roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));
            //    await _roleManager.CreateAsync(new IdentityRole(AppRoles.Employee));
            //    await _roleManager.CreateAsync(new IdentityRole(AppRoles.Customer));

            //    _logger.LogInformation("Initial Identity roles created.");
            //}

            Input = new InputModel()
            {
                RoleList = _roleManager.Roles.Select(r => r.Name).Select(n => new SelectListItem
                {
                    Text = n,
                    Value = n
                })
            };

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                user.PhoneNumber = Input.PhoneNumber;
                user.StreetAddress = Input.StreetAddress;
                user.City = Input.City;
                user.State = Input.State;
                user.PostalCode = Input.PostalCode;
                user.Name = Input.Name;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (Input.Role == null)
                    {
                        await _userManager.AddToRoleAsync(user, AppRoles.Customer);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, Input.Role);
                    }

                    // ✅ إنشاء كود التأكيد وإرسال الإيميل فعلياً في كل الحالات
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId, code, returnUrl },
                        protocol: Request.Scheme);

                    await _emailService.SendEmailAsync(Input.Email, "Confirm your email",
$@"
<html>
<head>
    <style>
        .button-style {{
            background-color: #FFC0CB; 
            color: #4B0082; 
            padding: 12px 25px;
            border-radius: 8px;
            text-decoration: none;
            font-weight: bold;
            display: inline-block;
            border: 1px solid #FFC0CB;
            transition: all 0.3s ease;
        }}
        .button-style:hover {{
            background-color: #FFB6C1; 
        }}
    </style>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 30px; margin: 0;'>
    <center>
        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;'>
            <tr>
                <td align='center'>
                    <div style='max-width: 600px; margin: auto; background: white; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden;'>
                        
                        <div style='background-color: #FFDAB9; padding: 25px; text-align: center; border-bottom: 2px solid #FFC0CB;'>
                            <h1 style='color: #4B0082; margin: 0; font-size: 24px;'>🍰 Sweet Shop 🍬</h1>
                        </div>

                        <div style='padding: 30px;'>
                            <h2 style='color: #333; margin-top: 0; font-size: 22px; text-align: center;'>Confirm Your New Account</h2>
                            
                            <p style='font-size: 16px; color: #555; line-height: 1.6;'>
                                Hello {Input.Name},
                            </p>
                            
                            <p style='font-size: 16px; color: #555; line-height: 1.6;'>
                                Thank you for joining the **Sweet Shop** family! To start exploring our delicious treats, please click the button below to confirm your email address:
                            </p>
                            
                            <div style='text-align: center; margin: 40px 0;'>
                                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='button-style'>
                                    Activate My Account
                                </a>
                            </div>

                            <p style='font-size: 16px; color: #555; line-height: 1.6;'>
                                If the button above does not work, please copy and paste the following link into your web browser:
                            </p>
                            <p style='font-size: 12px; color: #999; word-break: break-all;'>
                                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>{HtmlEncoder.Default.Encode(callbackUrl)}</a>
                            </p>
                        </div>
                        
                        <div style='background-color: #FFDAB9; padding: 15px; text-align: center; font-size: 12px; color: #8B4513;'>
                            <p style='margin: 0;'>
                                This is an automated email. If you didn’t create this account, please ignore this message.
                            </p>
                            <p style='margin: 5px 0 0;'>
                                &copy; {DateTime.Now.Year} Sweet Shop. All rights reserved.
                            </p>
                        </div>

                    </div>
                </td>
            </tr>
        </table>
    </center>
</body>
</html>
");

                    // ✅ دايمًا هنحول لصفحة تأكيد التسجيل بعد الإرسال
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
