using KS_Sweets.Domain.Entities.Common;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KS_Sweets.Domain.Entities.Identity
{
    /// <summary>
    /// Represents an application user with extended profile details.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // --- Profile Details ---
        /// <summary>
        /// Full name of the user.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Street address of the user.
        /// </summary>
        public string? StreetAddress { get; set; }

        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        /// <summary>
        /// Not mapped user role (used only in UI)
        /// </summary>
        [NotMapped]
        public string? Role { get; set; }

        // Properties from BaseAuditEntity are placed here, making this user model self-auditing.
        // NOTE: If your other Domain Models inherit BaseAuditEntity, you will have to populate
        // these properties manually, or using an IAuditable Interface approach.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
