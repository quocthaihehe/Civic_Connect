using System;
using System.ComponentModel.DataAnnotations;

namespace CivicConnect.Web.Models.Entities
{
    public class PointTransaction
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int PointsDelta { get; set; } // Thay đổi CitizenPoints
        public int TrustScoreDelta { get; set; } // Thay đổi TrustScore

        [Required]
        public string Reason { get; set; } // Lý do cộng/trừ điểm

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
