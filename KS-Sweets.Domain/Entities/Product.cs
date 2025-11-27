using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KS_Sweets.Domain.Entities
{
    /// <summary>
    /// Represents a product with pricing, stock, images, and category.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Unique identifier for the product (Primary Key).
        /// </summary>
        [Key]
        public int Id { get; set; }

        // --- Identification & Description ---
        /// <summary>
        /// The name of the product (e.g., Chocolate Fudge Cake).
        /// </summary>
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// URL-friendly identifier for the product page (e.g., chocolate-fudge-cake).
        /// </summary>
        [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug must be lowercase and can contain only letters, numbers, and hyphens")]
        public string? Slug { get; set; }

        /// <summary>
        /// Detailed description of the product.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        // --- Pricing ---
        /// <summary>
        /// Base selling price of the product.
        /// </summary>
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        /// <summary>
        /// Optional percentage discount applied to the product.
        /// </summary>
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        [Display(Name = "Discount (%)")]
        public double? Discount { get; set; } = 0;

        // --- Images ---
        /// <summary>
        /// Collection of images associated with this product.
        /// </summary>
        [ValidateNever]
        public List<ProductImage> ProductImages { get; set; } = [];

        /// <summary>
        /// Used for file upload in the form; not mapped to the database.
        /// </summary>
        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        // --- Inventory & Status ---
        /// <summary>
        /// Current quantity of the product in stock.
        /// </summary>
        [Display(Name = "Stock Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a positive number")]
        public int StockQuantity { get; set; }

        /// <summary>
        /// Flag indicating if the product is currently visible and available for sale.
        /// </summary>
        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        // --- Engagement ---
        /// <summary>
        /// Average customer rating for the product (0 to 5).
        /// </summary>
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public double Rating { get; set; } = 0;

        // --- Category Linkage ---
        /// <summary>
        /// Foreign Key linking the product to its category.
        /// </summary>
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Navigation property to the Category entity.
        /// </summary>
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }
    }
}