using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KS_Sweets.Domain.Entities
{
    /// <summary>
    /// Line items inside an order. This links a specific product and its quantity/price to the main OrderHeader.
    /// </summary>
    public class OrderDetails
    {
        /// <summary>
        /// Unique identifier for this order line item (Primary Key).
        /// </summary>
        public int Id { get; set; }

        // --- Order Header Linkage ---
        /// <summary>
        /// Foreign Key linking to the parent order header (Required).
        /// </summary>
        [Required]
        public int OrderHeaderId { get; set; }

        /// <summary>
        /// Navigation property to the main order summary.
        /// </summary>
        [ForeignKey("OrderHeaderId")]
        [ValidateNever]
        public OrderHeader OrderHeader { get; set; }

        // --- Product Linkage ---
        /// <summary>
        /// Foreign Key linking to the purchased product.
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Navigation property to the Product entity.
        /// </summary>
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        // --- Item Details ---
        /// <summary>
        /// The quantity of the product ordered.
        /// </summary>
        [Display(Name = "Quantity")]
        public int Count { get; set; }

        /// <summary>
        /// The final price of the product at the time of the order.
        /// </summary>
        public double Price { get; set; }
    }
}
