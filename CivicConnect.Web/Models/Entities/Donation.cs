using System;

namespace CivicConnect.Web.Models.Entities
{
    public class Donation
    {
        public int Id { get; set; }
        public int DonationCategoryId { get; set; }
        public string? UserId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public decimal Amount { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
        public bool IsAnonymous { get; set; } = false;
        public string? PayUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual DonationCategory DonationCategory { get; set; } = null!;
        public virtual ApplicationUser? User { get; set; }
    }
}
