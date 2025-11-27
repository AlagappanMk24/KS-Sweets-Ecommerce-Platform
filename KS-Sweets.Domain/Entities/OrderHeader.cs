using KS_Sweets.Domain.Entities.Common;
using KS_Sweets.Domain.Entities.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KS_Sweets.Domain.Entities
{
    /// <summary>
    /// Order summary header containing totals, user info, and shipping details.
    /// </summary>
    public class OrderHeader : BaseAuditEntity
    {
        /// <summary>
        /// Unique identifier for the order (Primary Key).
        /// </summary>
        [Key]
        public int Id { get; set; }

        // --- User Linkage ---
        /// <summary>
        /// Foreign Key linking the order to the purchasing user.
        /// </summary>
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// Navigation property for the purchasing user's details.
        /// </summary>
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        // --- Date and Total ---

        /// <summary>
        /// Date the order was placed.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Date the order was shipped.
        /// </summary
        public DateTime ShippingDate { get; set; }

        /// <summary>
        /// The final total cost of the order, including tax and shipping.
        /// </summary>
        public double OrderTotal { get; set; }

        // --- Status and Tracking ---

        /// <summary>
        /// Current status of the order (e.g., Pending, Processing, Shipped).
        /// </summary>
        public string? OrderStatus { get; set; }

        /// <summary>
        /// Status of the payment (e.g., Pending, Approved, Refunded).
        /// </summary>
        public string? PaymentStatus { get; set; }

        /// <summary>
        /// Tracking number assigned to the shipment.
        /// </summary>
        public string? TrackingNumber { get; set; }

        /// <summary>
        /// Carrier or courier service used for shipping (e.g., FedEx, USPS).
        /// </summary>
        public string? Carrier { get; set; }

        // --- Payment Details ---
        /// <summary>
        /// Date the payment was processed.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Date payment is due (used for deferred payments).
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime PaymentDueDate { get; set; }

        /// <summary>
        /// Stripe/PayPal Session ID for payment capture.
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// Stripe/PayPal Payment Intent ID for tracking transactions.
        /// </summary>
        public string? PaymentIntentId { get; set; }

        // --- Shipping Address ---
        /// <summary>
        /// Recipient's phone number.
        /// </summary>
        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Recipient's street address.
        /// </summary>
        [Required]
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }
        /// <summary>
        /// Recipient's city.
        /// </summary>
        [Required]
        public string City { get; set; }

        /// <summary>
        /// Recipient's state/province.
        /// </summary>
        [Required]
        public string State { get; set; }

        /// <summary>
        /// Recipient's postal/zip code.
        /// </summary>
        [Required]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Recipient's full name.
        /// </summary>
        [Required]
        [Display(Name = "Recipient Name")]
        public string Name { get; set; }
    }
}