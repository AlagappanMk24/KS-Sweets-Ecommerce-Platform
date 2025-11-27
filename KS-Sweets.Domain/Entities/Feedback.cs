using KS_Sweets.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace KS_Sweets.Domain.Entities
{
    /// <summary>
    /// Stores customer feedback and ratings.
    /// </summary>
    public class Feedback : BaseAuditEntity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, MaxLength(500)]
        public string Comment { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        /// <summary>
        /// Visibility control for admin approval.
        /// </summary>
        public bool IsApproved { get; set; } = false;
    }
}