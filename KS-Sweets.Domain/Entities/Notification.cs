using KS_Sweets.Domain.Entities.Common;
using KS_Sweets.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KS_Sweets.Domain.Entities
{
    /// <summary>
    /// Stores system notifications for users (order updates, alerts, etc.)
    /// </summary>
    public class Notification : BaseAuditEntity
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;
    }
}