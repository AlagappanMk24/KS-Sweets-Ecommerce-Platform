using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KS_Sweets.Domain.Entities
{
    /// <summary>
    /// Represents a single image associated with a product.
    /// </summary>
    public class ProductImage
    {
        /// <summary>
        /// Unique identifier for the image record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The URL where the image is stored (e.g., S3 bucket, server path).
        /// </summary>
        [Required]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Foreign Key linking the image back to its product.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Navigation property to the Product entity.
        /// </summary>
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}