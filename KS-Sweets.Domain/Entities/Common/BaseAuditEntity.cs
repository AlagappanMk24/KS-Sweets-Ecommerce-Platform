namespace KS_Sweets.Domain.Entities.Common
{
    /// <summary>
    /// Base entity for audit tracking (created/updated timestamps)
    /// </summary>
    public abstract class BaseAuditEntity
    {
        /// <summary>
        /// The date and time when record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when record was last modified.
        /// </summary>
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The user ID of the user who created the record.
        /// </summary>
        // Assuming you are using ApplicationUser, the ID type is typically string or Guid.
        public string? CreatedBy { get; set; }

        /// <summary>
        /// The user ID of the user who last modified the record.
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Used to soft-delete data without removing from DB.
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }

}
