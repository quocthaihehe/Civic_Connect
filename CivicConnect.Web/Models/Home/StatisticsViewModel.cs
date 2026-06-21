using System.Collections.Generic;

namespace CivicConnect.Web.Models.Home
{
    public class StatisticsViewModel
    {
        public int TotalIssues { get; set; }
        public double ResolvedPercentage { get; set; }
        public double AverageProcessingHours { get; set; }
        public Dictionary<string, int> IssuesByCategory { get; set; }
    }
}
