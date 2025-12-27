using KS_Sweets.Application.Contracts.DTOs.CategoryDTOs;
using KS_Sweets.Application.Contracts.DTOs.CategoryDTOS;
using KS_Sweets.Domain.Entities;
using KS_Sweets.Domain.Models.Request;
using KS_Sweets.Domain.Models.Response;

namespace KS_Sweets.Application.Contracts.Services
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAllCategories();
        Category? GetCategoryBySlug(string slug);
        Category? GetCategoryById(int id);
        CategoryEditDto? GetCategoryByIdDto(int id);
        Category GetCategoryForCustomerView(string slug);
        bool CreateCategory(CategoryCreateDto category);
        bool UpdateCategory(CategoryEditDto request);
        bool DeleteCategory(int id);
        bool RemoveCategoryImage(int categoryId);
        bool ToggleStatus(int id);
        bool BulkUpdateStatus(int[] ids, bool isActive);
        bool BulkDelete(int[] ids);

        // Server-side DataTables
        DataTablesResponse<CategoryListDto> GetCategoriesForDataTable(DataTablesRequest request);
    }
}
