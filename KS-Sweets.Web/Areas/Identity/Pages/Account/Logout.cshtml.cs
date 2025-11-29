// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using KS_Sweets.Domain.Constants;
using KS_Sweets.Domain.Entities.Identity;
using KS_Sweets.Web.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KS_Sweets.Web.Areas.Identity.Pages.Account
{
    public class LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger) : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly ILogger<LogoutModel> _logger = logger;

        /// <summary>
        /// Handles the POST request for user logout.
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to after successful logout.</param>
        /// <returns>An action result representing the redirection.</returns>
        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            // 1. Sign out the user from ASP.NET Core Identity
            await _signInManager.SignOutAsync();

            // 2. Clear application-specific session data (ShoppingCart and Wishlist counts)
            HttpContext.Session.Remove(SessionKeys.ShoppingCart);
            HttpContext.Session.Remove(SessionKeys.WishlistCount);

            // 3. Log the successful logout and session clear
            _logger.LogInformation("User logged out and session cleared.");

            // 4. Handle redirection
            if (returnUrl != null)
            {
                // Redirect to the specified return URL if provided (LocalRedirect ensures safety)
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Redirect to home page if no specific returnUrl is given (better user experience than staying on the logout page)
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }
        }
    }
}
