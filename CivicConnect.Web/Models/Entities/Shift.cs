using System;

namespace CivicConnect.Web.Models.Entities
{
    public class Shift
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }
        
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
