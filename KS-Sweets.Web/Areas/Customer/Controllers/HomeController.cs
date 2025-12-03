using KS_Sweets.Application.Contracts.Services;
using KS_Sweets.Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KS_Sweets.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController(ILogger<HomeController> logger,
                          ICustomerService customerService) : Controller
    {
        private readonly ICustomerService _customerService = customerService;
        private readonly ILogger<HomeController> _logger = logger;

        public IActionResult Index()
        {
            string? userId = User.Identity.IsAuthenticated
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            var data = _customerService.LoadHomePage(userId);

            if (userId != null)
            {
                HttpContext.Session.SetInt32(SessionKeys.ShoppingCart, data.CartCount);
                HttpContext.Session.SetInt32(SessionKeys.WishlistCount, data.WishlistCount);
            }

            ViewBag.Feedbacks = data.Feedbacks;

            return View(data.Categories);
        }

        public IActionResult Category(string slug)
        {
            var category = _customerService.GetCategory(slug);

            if (category == null) return NotFound();

            return View(category);
        }
    }
}
