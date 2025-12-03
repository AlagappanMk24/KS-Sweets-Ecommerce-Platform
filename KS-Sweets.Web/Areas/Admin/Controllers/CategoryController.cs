using KS_Sweets.Application.Contracts.Services;
using KS_Sweets.Domain.Constants;
using KS_Sweets.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KS_Sweets.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public class CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger) : Controller
    {
        private readonly ICategoryService _categoryService = categoryService;
        private readonly ILogger<CategoryController> _logger = logger;

        // -------------------- INDEX --------------------
        public IActionResult Index()
        {
            try
            {
                var categories = _categoryService.GetAllCategories();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories");
                return View("Error");
            }
        }

        // -------------------- DETAILS --------------------
        public IActionResult Details(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                    return NotFound();

                var category = _categoryService.GetCategoryBySlug(slug);

                if (category == null)
                    return NotFound();

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading details for slug: {slug}");
                return View("Error");
            }
        }

        // -------------------- ADD CATEGORY (GET) --------------------
        public IActionResult Add() => View();

        // -------------------- ADD CATEGORY (POST) --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            bool success = _categoryService.CreateCategory(category, category.ImageFile);

            if (!success)
            {
                TempData["error"] = "Failed to create category.";
                return View(category);
            }

            TempData["success"] = "Category created!";
            return RedirectToAction("Index");
        }


        // -------------------- EDIT (GET) --------------------
        public IActionResult Edit(string slug)
        {
            var category = _categoryService.GetCategoryBySlug(slug);
            if (category == null) return NotFound();

            return View(category);
        }

        // -------------------- EDIT (POST) --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            bool success = _categoryService.UpdateCategory(category, category.ImageFile);
            if (!success)
            {
                TempData["error"] = "Update failed.";
                return View(category);
            }

            TempData["success"] = "Category updated!";
            return RedirectToAction("Index");
        }

        // -------------------- DELETE --------------------
        [HttpPost]
        public IActionResult Delete(int id)
        {
            bool success = _categoryService.DeleteCategory(id);

            return Json(new { success });
        }

        // -------------------- GET ALL (JSON for DataTables) --------------------
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var categories = _categoryService.GetAllCategories();
                return Json(new { data = categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories for GetAll");
                return Json(new { data = new List<Category>() });
            }
        }
    }
}