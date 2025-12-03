using KS_Sweets.Application.Contracts.Persistence;
using KS_Sweets.Application.Contracts.Services;
using KS_Sweets.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KS_Sweets.Infrastructure.Services
{
    public class CategoryService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, ILogger<CategoryService> logger) : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly ILogger<CategoryService> _logger = logger;
        public IEnumerable<Category> GetAllCategories()
        {
            return _unitOfWork.Categories.GetAll(includeProperties: "Products");
        }
        public Category GetCategoryBySlug(string slug)
        {
            return _unitOfWork.Categories.Get(x => x.Slug == slug, includeProperties: "Products");
        }
        public Category GetCategoryForCustomerView(string slug)
        {
            return _unitOfWork.Categories.Get(
                c => c.Slug == slug,
                includeProperties: "Products,Products.ProductImages");
        }
        public bool CreateCategory(Category category, IFormFile? imageFile)
        {
            try
            {
                category.Slug = category.Name.ToLower().Replace(" ", "-");

                var exists = _unitOfWork.Categories.Get(c =>
                     c.Name == category.Name || c.Slug == category.Slug);

                if (exists != null)
                    throw new Exception("Category with same name or slug already exists.");

                if (imageFile != null)
                    category.ImageUrl = SaveImage(imageFile);

                _unitOfWork.Categories.Add(category);
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
        public bool UpdateCategory(Category category, IFormFile? imageFile)
        {
            try
            {
                category.Slug = category.Name.ToLower().Replace(" ", "-");

                var exists = _unitOfWork.Categories.Get(c =>
                     (c.Name == category.Name || c.Slug == category.Slug)
                     && c.Id != category.Id);

                if (exists != null)
                    throw new Exception("Category already exists!");

                if (imageFile != null)
                {
                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, category.ImageUrl.TrimStart('/'));
                        if (File.Exists(oldPath)) File.Delete(oldPath);
                    }
                    category.ImageUrl = SaveImage(imageFile);
                }

                _unitOfWork.Categories.Update(category);
                _unitOfWork.Save();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateCategory failed");
                return false;
            }
        }
        public bool DeleteCategory(int id)
        {
            try
            {
                var category = _unitOfWork.Categories.Get(c => c.Id == id);
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
    }
}
