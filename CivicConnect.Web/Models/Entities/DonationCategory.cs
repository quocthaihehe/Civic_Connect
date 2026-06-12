using System;
using System.Collections.Generic;

namespace CivicConnect.Web.Models.Entities
{
    public class DonationCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }
}
