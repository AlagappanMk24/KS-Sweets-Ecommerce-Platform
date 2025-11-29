namespace KS_Sweets.Domain.Constants
{
    /// <summary>
    /// Static constants for payment transaction statuses.
    /// </summary>
    public static class PaymentStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string DelayedPayment = "ApprovedForDelayedPayment";
        public const string Rejected = "Rejected";
    }
}