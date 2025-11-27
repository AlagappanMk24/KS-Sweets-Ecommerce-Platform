using KS_Sweets.Domain.Entities.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KS_Sweets.Domain.Entities
{
    /// <summary>
    /// Represents a single line item in a user's shopping cart.
    /// </summary>
    public class ShoppingCart
    {
        /// <summary>
        /// Unique identifier for the cart item (Primary Key).
        /// </summary>
        public int Id { get; set; }

        // --- Product Linkage ---
        /// <summary>
        /// Foreign Key linking the cart item to a specific product.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Navigation property for the Product entity.
        /// </summary>
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        /// <summary>
        /// The quantity of the product the user wishes to purchase.
        /// </summary>
        [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000")]
        public int Count { get; set; }

        // --- User Linkage ---
        /// <summary>
        /// Foreign Key linking the cart item to the user who owns the cart.
        /// </summary>
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// Navigation property for the ApplicationUser entity.
        /// </summary>
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// The calculated price of the product (not mapped to DB, used for display logic).
        /// </summary>
        [NotMapped]
        public double Price { get; set; }
    }
}
