namespace KS_Sweets.Domain.Constants
{
    /// <summary>
    /// Static constants for order fulfillment statuses.
    /// </summary>
    public static class OrderStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Cancelled = "Cancelled";
        public const string Refunded = "Refunded";
    }
}