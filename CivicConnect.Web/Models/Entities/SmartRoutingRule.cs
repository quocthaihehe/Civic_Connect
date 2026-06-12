using CivicConnect.Web.Models.Enums;
using System;

namespace CivicConnect.Web.Models.Entities
{
    public class SmartRoutingRule
    {
        public int Id { get; set; }
        public IssueCategory Category { get; set; }
        public string? DistrictCode { get; set; }
        public string? WardCode { get; set; }
        
        public string TargetUnitId { get; set; }
        public GovernmentUnit? TargetUnit { get; set; }
        
        public int SLADays { get; set; } = 5;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
