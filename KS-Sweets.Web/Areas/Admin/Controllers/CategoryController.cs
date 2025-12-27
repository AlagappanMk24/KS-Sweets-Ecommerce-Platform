
using Humanizer;
using KS_Sweets.Application.Contracts.DTOs.CategoryDTOs;
using KS_Sweets.Application.Contracts.DTOs.CategoryDTOS;
using KS_Sweets.Application.Contracts.Services;
using KS_Sweets.Domain.Constants;
using KS_Sweets.Domain.Entities;
using KS_Sweets.Domain.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KS_Sweets.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    [Route("admin/categories")]
    public class CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger) : Controller
    {
        private readonly ICategoryService _categoryService = categoryService;
        private readonly ILogger<CategoryController> _logger = logger;

        // -------------------- INDEX --------------------
        // GET: /admin/categories
        [HttpGet("")]
        public IActionResult Index() => View();

        // -------------------- DETAILS --------------------

        // GET: /admin/categories/{slug}
        [HttpGet("{slug}")]
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

        // ===================== AJAX =====================

        // -------------------- GET ALL (JSON for DataTables) --------------------

        // POST: /admin/categories/list
        [HttpPost("list")]
        public IActionResult List([FromBody] DataTablesRequest request)
        {
            // If request is null (rare edge case), fallback to empty request
            request ??= new DataTablesRequest();
            var response = _categoryService.GetCategoriesForDataTable(request);
            return Json(new
            {
                draw = response.Draw,
                recordsTotal = response.RecordsTotal,
                recordsFiltered = response.RecordsFiltered,
                data = response.Data
            });
        }

        // GET: /admin/categories/{id}
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var dto = _categoryService.GetCategoryByIdDto(id);

            if (dto == null)
                return NotFound(new { success = false, message = "Category not found" });

            return Ok(new { success = true, data = dto });
        }

        // -------------------- ADD CATEGORY (POST) --------------------
        // POST: /admin/categories
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public IActionResult Add([FromForm] CategoryCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return Json(new { success = false, errors = errors });
            }

            bool success = _categoryService.CreateCategory(model);

            return Json(new
            {
                success = success,
                message = success ? "Category created" : "Failed to create category"
            });
        }

        // -------------------- EDIT (POST) --------------------
        // POST: /admin/categories/{id}
        [HttpPost("{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryEditDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return Json(new { success = false, errors = errors });
            }

            bool success = _categoryService.UpdateCategory(dto);
            return Json(new
            {
                success,
                message = success ? "Category updated" : "Update failed"
            });
        }

        // POST: /admin/categories/{id}/toggle-status
        [HttpPost("{id:int}/toggle-status")]
        public IActionResult ToggleStatus(int id)
        {
            var success = _categoryService.ToggleStatus(id);
            var category = _categoryService.GetCategoryById(id);
            return Json(new { success, isActive = category?.IsActive });
        }

        // -------------------- DELETE --------------------
        // POST: /admin/categories/{id}/delete
        [HttpPost("{id:int}/delete")]
        public IActionResult Delete(int id)
        {
            bool success = _categoryService.DeleteCategory(id);

            return Json(new { success });
        }

        // POST: /admin/categories/bulk-delete
        [HttpPost("bulk-delete")]
        public IActionResult BulkDelete(int[] ids)
        {
            var success = _categoryService.BulkDelete(ids);
            return Json(new
            {
                success,
                message = success ? $"{ids.Length} categories deleted" : "Delete failed"
            });
        }

        // POST: /admin/categories/{id}/remove-image
        [HttpPost("{id:int}/remove-image")]
        public IActionResult RemoveImage(int id)
        {
            var success = _categoryService.RemoveCategoryImage(id);

            return Json(new
            {
                success,
                message = success ? "Image removed successfully" : "Failed to remove image"
            });
        }

        // POST: /admin/categories/bulk-status
        [HttpPost("bulk-status")]
        public IActionResult BulkStatus([FromBody] BulkStatusDto dto)
        {
            if (dto?.Ids == null || dto.Ids.Length == 0)
                return BadRequest(new { success = false, message = "No categories selected" });

            var success = _categoryService.BulkUpdateStatus(dto.Ids, dto.IsActive);

            return Json(new
            {
                success,
                message = success
                    ? $"{dto.Ids.Length} categories {(dto.IsActive ? "activated" : "deactivated")}"
                    : "Bulk update failed"
            });
        }
    }
}