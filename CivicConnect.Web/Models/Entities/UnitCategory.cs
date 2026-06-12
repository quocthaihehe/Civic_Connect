using CivicConnect.Web.Models.Enums;

namespace CivicConnect.Web.Models.Entities
{
    public class UnitCategory
    {
        public string GovernmentUnitId { get; set; }
        public GovernmentUnit GovernmentUnit { get; set; }
        public IssueCategory Category { get; set; }
    }
}
