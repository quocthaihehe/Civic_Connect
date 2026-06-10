using CivicConnect.Core.Enums;

namespace CivicConnect.Core.Entities
{
    public class UnitCategory
    {
        public string GovernmentUnitId { get; set; }
        public GovernmentUnit GovernmentUnit { get; set; }
        public IssueCategory Category { get; set; }
    }
}
