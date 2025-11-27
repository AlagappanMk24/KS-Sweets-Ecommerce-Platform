using KS_Sweets.Domain.Entities.Common;
using KS_Sweets.Domain.Entities.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace KS_Sweets.Domain.Entities
{
    /// <summary>
    /// Represents a single item in a user's wish list, linking a product to a user.
    /// </summary>
    public class WishList : BaseAuditEntity
    {
        /// <summary>
        /// Primary Key for the WishList item.
        /// </summary>
        public int Id { get; set; }

        // --- Product Linkage ---
        /// <summary>
        /// Foreign Key linking to the Product.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Navigation property for the Product entity.
        /// </summary>
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        // --- User Linkage ---
        /// <summary>
        /// Foreign Key linking to the ApplicationUser (the user who owns the wishlist).
        /// </summary>
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// Navigation property for the ApplicationUser entity.
        /// </summary>
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
