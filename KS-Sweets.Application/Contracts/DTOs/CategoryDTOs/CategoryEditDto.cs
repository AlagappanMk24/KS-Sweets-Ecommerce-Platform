using Microsoft.AspNetCore.Http;

namespace KS_Sweets.Application.Contracts.DTOs.CategoryDTOs
{
    public class CategoryEditDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public bool RemoveImage { get; set; }
    }

}
