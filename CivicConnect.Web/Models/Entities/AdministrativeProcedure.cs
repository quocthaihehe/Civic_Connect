using System;
using System.ComponentModel.DataAnnotations;

namespace CivicConnect.Web.Models.Entities
{
    public class AdministrativeProcedure
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Code { get; set; } = string.Empty; // Mã thủ tục

        [MaxLength(200)]
        public string LegalBasis { get; set; } = string.Empty; // Cơ sở pháp lý

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty; // Lĩnh vực (VD: Tư pháp, Hộ tịch)

        public string Description { get; set; } = string.Empty; // Mô tả ngắn gọn

        public string RequiredDocuments { get; set; } = string.Empty; // Hồ sơ cần chuẩn bị (dạng Text/HTML)

        [MaxLength(100)]
        public string ProcessingTime { get; set; } = string.Empty; // Thời gian giải quyết (VD: 3 ngày làm việc)

        [MaxLength(100)]
        public string Fee { get; set; } = string.Empty; // Phí, lệ phí

        [MaxLength(255)]
        public string SubmissionPlace { get; set; } = string.Empty; // Nơi tiếp nhận hồ sơ

        [MaxLength(500)]
        public string TemplateUrl { get; set; } = string.Empty; // Link tải biểu mẫu

        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
