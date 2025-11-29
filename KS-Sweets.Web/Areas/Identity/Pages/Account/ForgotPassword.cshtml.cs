//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//#nullable disable

//using System.ComponentModel.DataAnnotations;
//using System.Text;
//using System.Text.Encodings.Web;
//using KS_Sweets.Application.Contracts.Services;
//using KS_Sweets.Domain.Entities.Identity;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.AspNetCore.WebUtilities;

//namespace KS_Sweets.Web.Areas.Identity.Pages.Account
//{
//    public class ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailService emailService) : PageModel
//    {
//        private readonly UserManager<ApplicationUser> _userManager = userManager;
//        private readonly IEmailService _emailService = emailService;

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        [BindProperty]
//        public InputModel Input { get; set; }

//        /// <summary>
//        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//        ///     directly from your code. This API may change or be removed in future releases.
//        /// </summary>
//        public class InputModel
//        {
//            /// <summary>
//            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
//            ///     directly from your code. This API may change or be removed in future releases.
//            /// </summary>
//            [Required]
//            [EmailAddress]
//            public string Email { get; set; }
//        }

//        public async Task<IActionResult> OnPostAsync()
//        {
//            if (ModelState.IsValid)
//            {
//                var user = await _userManager.FindByEmailAsync(Input.Email);
//                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
//                {
//                    // Don't reveal that the user does not exist or is not confirmed
//                    return RedirectToPage("./ForgotPasswordConfirmation");
//                }

//                // For more information on how to enable account confirmation and password reset please
//                // visit https://go.microsoft.com/fwlink/?LinkID=532713
//                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
//                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
//                var callbackUrl = Url.Page(
//                    "/Account/ResetPassword",
//                    pageHandler: null,
//                    values: new { area = "Identity", code },
//                    protocol: Request.Scheme);

//                await _emailService.SendEmailAsync(
//                    Input.Email,
//                    "Reset Password",
//                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

//                return RedirectToPage("./ForgotPasswordConfirmation");
//            }

//            return Page();
//        }
//    }
//}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using KS_Sweets.Application.Contracts.Services;
using KS_Sweets.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace KS_Sweets.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailService emailService) : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IEmailService _emailService = emailService;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                // We use a safe return path: Don't reveal that the user does not exist or is not confirmed.
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                // --- START OF NEW HTML EMAIL TEMPLATE ---
                string emailBody = $@"
                    <html>
                    <head>
                        <style>
                            .button-style {{
                                background-color: #FFC0CB; /* Soft Pink */
                                color: #4B0082; /* Indigo/Purple */
                                padding: 12px 25px;
                                border-radius: 8px;
                                text-decoration: none;
                                font-weight: bold;
                                display: inline-block;
                                border: 1px solid #FFC0CB;
                                transition: all 0.3s ease;
                            }}
                            .button-style:hover {{
                                background-color: #FFB6C1; /* Slightly darker pink */
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
                                                <h1 style='color: #4B0082; margin: 0; font-size: 24px;'>🍰 KS-Sweets Sweet Shop 🍬</h1>
                                            </div>

                                            <div style='padding: 30px;'>
                                                <h2 style='color: #333; margin-top: 0; font-size: 22px; text-align: center;'>Reset Your Password</h2>
                    
                                                <p style='font-size: 16px; color: #555; line-height: 1.6;'>
                                                    We received a request to reset the password for the account associated with this email address.
                                                </p>
                    
                                                <p style='font-size: 16px; color: #555; line-height: 1.6;'>
                                                    If you initiated this request, please click the button below to set a new password:
                                                </p>
                    
                                                <div style='text-align: center; margin: 40px 0;'>
                                                    <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='button-style'>
                                                        Set New Password
                                                    </a>
                                                </div>

                                                <p style='font-size: 16px; color: #555; line-height: 1.6;'>
                                                    This link is only valid for a limited time. If you didn't request a password reset, you can safely ignore this email.
                                                </p>

                                                <p style='font-size: 16px; color: #555; line-height: 1.6;'>
                                                    If the button above does not work, please copy and paste the following link into your web browser:
                                                </p>
                                                <p style='font-size: 12px; color: #999; word-break: break-all;'>
                                                    <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>{HtmlEncoder.Default.Encode(callbackUrl)}</a>
                                                </p>
                                            </div>
                    
                                            <div style='background-color: #FFDAB9; padding: 15px; text-align: center; font-size: 12px; color: #8B4513;'>
                                                <p style='margin: 0;'>
                                                    This is an automated email. Please do not reply.
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
                    ";
                // --- END OF NEW HTML EMAIL TEMPLATE ---

                await _emailService.SendEmailAsync(
                    Input.Email,
                    "Password Reset Request", // Subject
                    emailBody // HTML Content
                );

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}