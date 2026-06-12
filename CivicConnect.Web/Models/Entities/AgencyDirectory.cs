using System;
using System.ComponentModel.DataAnnotations;

namespace CivicConnect.Web.Models.Entities
{
    public class AgencyDirectory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Type { get; set; } = string.Empty; // Loại cơ quan: Công an, Y tế, Điện lực...

        [MaxLength(50)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string WorkingHours { get; set; } = "Thứ 2 - Thứ 6: 08:00 - 17:00";

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public float Rating { get; set; } = 5.0f; // Đánh giá sao

        public string ReceptionSchedule { get; set; } = string.Empty; // Thông tin lịch tiếp dân (Text mô tả)

        public bool IsEmergency { get; set; } = false; // Phân loại khẩn cấp (vd 113, 114, 115)

        public int OrderIndex { get; set; } = 0; // Sắp xếp hiển thị
    }
}
