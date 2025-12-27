using AutoMapper;
using KS_Sweets.Application.Contracts.DTOs.CategoryDTOs;
using KS_Sweets.Application.Contracts.DTOs.CategoryDTOS;
using KS_Sweets.Application.Contracts.Persistence;
using KS_Sweets.Application.Contracts.Services;
using KS_Sweets.Domain.Entities;
using KS_Sweets.Domain.Models.Request;
using KS_Sweets.Domain.Models.Response;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace KS_Sweets.Infrastructure.Services
{
    public class CategoryService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, IMapper mapper, ILogger<CategoryService> logger) : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<CategoryService> _logger = logger;
        public IEnumerable<Category> GetAllCategories()
        {
            return _unitOfWork.Categories.GetAll(includeProperties: "Products");
        }
        public Category? GetCategoryById(int id)
        {
            return _unitOfWork.Categories.Get(c => c.Id == id);
        }
        public CategoryEditDto? GetCategoryByIdDto(int id)
        {
            var category = _unitOfWork.Categories.Get(x => x.Id == id);

            if (category == null)
                return null;

            return _mapper.Map<CategoryEditDto>(category);
        }
        public Category? GetCategoryBySlug(string slug)
        {
            return _unitOfWork.Categories.Get(x => x.Slug == slug, includeProperties: "Products");
        }
        public Category GetCategoryForCustomerView(string slug)
        {
            return _unitOfWork.Categories.Get(
                c => c.Slug == slug,
                includeProperties: "Products,Products.ProductImages");
        }
        public bool CreateCategory(CategoryCreateDto request)
        {
            try
            {
                // 1️ Check Duplicate Name
                if (_unitOfWork.Categories.ExistsByName(request.Name))
                    throw new Exception("Category name already exists.");

                // 2️ Generate slug 
                var slug = GenerateSlug(request.Name);

                // 3️ Check Duplicate Slug
                if (_unitOfWork.Categories.ExistsBySlug(slug))
                    throw new Exception("Category slug already exists.");

                // 4️ Use AutoMapper to Map DTO -> Entity
                var entity = _mapper.Map<Category>(request);

                // 5️ Save Image
                if (request.ImageFile != null)
                    entity.ImageUrl = SaveImage(request.ImageFile);

                // 6️ Save to DB
                _unitOfWork.Categories.Add(entity);
                _unitOfWork.Save();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateCategory failed");
                return false;
            }
        }
        private string SaveImage(IFormFile image)
        {
            string root = _webHostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
            string path = Path.Combine(root, "images", "categories");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create);
            image.CopyTo(stream);

            return "/images/categories/" + fileName;
        }
        public bool UpdateCategory(CategoryEditDto request)
        {
            try
            {
                var existing = GetCategoryById(request.Id);
                if (existing == null)
                    return false;

                // Duplicate Name
                if (_unitOfWork.Categories.ExistsByName(request.Name, request.Id))
                    throw new Exception("Category name already exists");

                // Generate slug
                string slug = GenerateSlug(request.Name);

                // Duplicate slug
                if (_unitOfWork.Categories.ExistsBySlug(slug, request.Id))
                    throw new Exception("Category slug already exists");

                // Map dto → entity
                _mapper.Map(request, existing);

                // Handle image
                if (request.ImageFile != null && request.ImageFile.Length > 0)
                {
                    // Replace image
                    existing.ImageUrl = SaveImage(request.ImageFile);
                }
                else
                {
                    // Keep existing image
                    existing.ImageUrl = request.ImageUrl;
                }

                existing.UpdatedAt = DateTime.Now;

                _unitOfWork.Categories.Update(existing);
                _unitOfWork.Save();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", request.Id);
                return false;
                // DO NOT THROW AGAIN — controller handles error
            }
        }
        public bool DeleteCategory(int id)
        {
            try
            {
                var category = GetCategoryById(id);
                if (category == null) return false;

                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, category.ImageUrl.TrimStart('/'));
                    if (File.Exists(oldPath)) File.Delete(oldPath);
                }

                _unitOfWork.Categories.Remove(category);
                _unitOfWork.Save();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteCategory failed");
                return false;
            }
        }
        public bool RemoveCategoryImage(int categoryId)
        {
            var category = GetCategoryById(categoryId);
            if (category == null || string.IsNullOrEmpty(category.ImageUrl))
                return false;

            // Delete physical file
            DeleteImageFile(category.ImageUrl);

            // Remove from DB
            category.ImageUrl = null;
            category.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Categories.Update(category);
            _unitOfWork.Save();

            return true;
        }
        public bool ToggleStatus(int id)
        {
            try
            {
                var category = GetCategoryById(id);
                if (category == null) return false;

                category.IsActive = !category.IsActive;
                category.UpdatedAt = DateTime.Now;

                _unitOfWork.Categories.Update(category);
                _unitOfWork.Save();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for category {Id}", id);
                return false;
            }
        }
        public bool BulkUpdateStatus(int[] ids, bool isActive)
        {
            try
            {
                _unitOfWork.Categories.BulkUpdateStatus(ids, isActive);
                _unitOfWork.Save();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk status update failed");
                return false;
            }
        }

        public bool BulkDelete(int[] ids)
        {
            if (ids == null || ids.Length == 0) return false;

            try
            {
                foreach (var id in ids)
                {
                    DeleteCategory(id);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk delete failed");
                return false;
            }
        }
        public DataTablesResponse<CategoryListDto> GetCategoriesForDataTable(DataTablesRequest request)
        {
            var baseQuery = _unitOfWork.Categories
                                 .GetAll(includeProperties: "Products")
                                 .AsQueryable();

            var totalRecords = baseQuery.Count();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search?.Value))
            {
                var search = request.Search.Value.ToLower();
                baseQuery = baseQuery.Where(c =>
                    c.Name.ToLower().Contains(search) ||
                    (c.Description != null && c.Description.ToLower().Contains(search)));
            }

            var filteredRecords = baseQuery.Count();

            // Sorting 
            if (request.Order != null && request.Order.Any())
            {
                var order = request.Order[0];
                var columnName = request.Columns[order.Column].Name?.ToLower();

                baseQuery = columnName switch
                {
                    "name" => order.Dir == "asc"
                        ? baseQuery.OrderBy(c => c.Name)
                        : baseQuery.OrderByDescending(c => c.Name),

                    "createdat" => order.Dir == "asc"
                        ? baseQuery.OrderBy(c => c.CreatedAt)
                        : baseQuery.OrderByDescending(c => c.CreatedAt),

                    "isactive" => order.Dir == "asc"
                        ? baseQuery.OrderBy(c => c.IsActive)
                        : baseQuery.OrderByDescending(c => c.IsActive),

                    _ => baseQuery.OrderByDescending(c => c.CreatedAt)
                };
            }
            else
            {
                // ✅ Default: Latest first
                baseQuery = baseQuery.OrderByDescending(c => c.CreatedAt);
            }

            // 📄 PAGINATION + PROJECTION
            var data = baseQuery
                        .Skip(request.Start)
                        .Take(request.Length)
                        .Select(c => new CategoryListDto
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Description = c.Description,
                            ImageUrl = c.ImageUrl,
                            ItemCount = c.Products.Count(),
                            IsActive = c.IsActive,
                            Slug = c.Slug,
                            CreatedAt = c.CreatedAt,
                            UpdatedAt = c.UpdatedAt
                        })
                        .ToList();

            return new DataTablesResponse<CategoryListDto>
            {
                Draw = request.Draw,
                RecordsTotal = totalRecords,
                RecordsFiltered = filteredRecords,
                Data = data
            };
        }
        public static string GenerateSlug(string text)
        {
            return Regex.Replace(
                text.ToLower().Trim(),
                @"[^a-z0-9]+",
                "-"
            ).Trim('-');
        }
        private void DeleteImageFile(string imageUrl)
        {
            var filePath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                imageUrl.TrimStart('/')
            );

            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}