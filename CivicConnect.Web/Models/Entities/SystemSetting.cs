using System;
using System.ComponentModel.DataAnnotations;

namespace CivicConnect.Web.Models.Entities
{
    public class SystemSetting
    {
        [Key]
        public string SettingKey { get; set; } // e.g. "MaintenanceMode", "OrgName", "LogoUrl"
        public string? SettingValue { get; set; }
        public string? Description { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
