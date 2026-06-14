using System.Collections.Generic;

namespace CivicConnect.Web.Models.Ai
{
    public class PolicySummaryResult
    {
        public bool IsSuccess { get; set; }
        public string ShortSummary { get; set; } = string.Empty;
        public List<string> BulletPoints { get; set; } = new();
        public string RealWorldExample { get; set; } = string.Empty;
        public string ModelUsed { get; set; } = string.Empty;
        public int TokensUsed { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
