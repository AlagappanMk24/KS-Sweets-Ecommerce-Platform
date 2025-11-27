using KS_Sweets.Domain.Entities.Common;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KS_Sweets.Domain.Entities
{
    public class Category : BaseAuditEntity
    {
        // Primary Key
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of the category. (e.g., Cake, Donut...)
        /// </summary>
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// URL-friendly identifier for SEO.
        /// </summary>
        [StringLength(100)]
        public string? Slug { get; set; }

        /// <summary>
        /// Description of the category.
        /// </summary>
        [MaxLength(250)]
        public string? Description { get; set; }

        /// <summary>
        /// Category image path.
        /// </summary>   
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Image upload object (not saved in DB). Used for file upload in the form, not stored in the database
        /// </summary>
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        /// <summary>
        /// Is the section available or hidden
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Number of products within the section
        /// This property will not be mapped to a database column
        /// </summary>
        [Range(0, int.MaxValue)]
        [NotMapped]
        public int ItemCount => Products?.Count ?? 0;

        /// <summary>
        /// Navigation: List of products under this category.
        /// </summary>
        public List<Product> Products { get; set; } = [];
    }
}