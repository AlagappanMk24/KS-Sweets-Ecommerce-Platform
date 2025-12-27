using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KS_Sweets.Application.Contracts.DTOs.CategoryDTOS
{
    public class CategoryCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public IFormFile ImageFile { get; set; }
    }

}
