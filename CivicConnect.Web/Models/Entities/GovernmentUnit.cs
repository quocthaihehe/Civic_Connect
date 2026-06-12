using CivicConnect.Web.Models.Enums;
using System.Collections.Generic;

namespace CivicConnect.Web.Models.Entities
{
    public class GovernmentUnit
    {
        public string Id { get; set; }                    // VD: "UBND_Q1_HCM"
        public string Name { get; set; }                  // VD: "UBND Quận 1"
        public GovernmentUnitType Type { get; set; }      // Cấp/Loại cơ quan
        
        public string? ParentUnitId { get; set; }         // Đơn vị cấp trên
        public GovernmentUnit? ParentUnit { get; set; }
        
        public string ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? WardCode { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<GovernmentUnit> ChildUnits { get; set; } = new List<GovernmentUnit>();
        public ICollection<ApplicationUser> Officials { get; set; } = new List<ApplicationUser>();
        public ICollection<Issue> AssignedIssues { get; set; } = new List<Issue>();
        
        // Loại vấn đề mà đơn vị này có thẩm quyền xử lý
        public ICollection<UnitCategory> Categories { get; set; } = new List<UnitCategory>();
    }
}
