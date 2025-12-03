using KS_Sweets.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace KS_Sweets.Application.Contracts.Services
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAllCategories();
        Category GetCategoryBySlug(string slug);
        Category GetCategoryForCustomerView(string slug);
        bool CreateCategory(Category category, IFormFile? imageFile);
        bool UpdateCategory(Category category, IFormFile? imageFile);
        bool DeleteCategory(int id);
    }
}
