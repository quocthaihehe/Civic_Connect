using System;

namespace CivicConnect.Web.Models.Entities
{
    public class PolicyAiSummary
    {
        public int Id { get; set; }
        public int PolicyId { get; set; }
        public Policy Policy { get; set; } = null!;

        public string ShortSummary { get; set; } = string.Empty;
        public string BulletPointsJson { get; set; } = "[]";   // JSON array string
        public string RealWorldExample { get; set; } = string.Empty;

        public string AiModel { get; set; } = string.Empty;
        public int TokensUsed { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
