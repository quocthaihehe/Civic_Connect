using System;

namespace CivicConnect.Web.Models.Entities
{
    public class IssueImage
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public Issue Issue { get; set; }
        public string PublicId { get; set; }        // Cloudinary public_id (để xóa khi cần)
        public string Url { get; set; }             // URL đầy đủ của ảnh
        public string ThumbnailUrl { get; set; }     // URL thumbnail từ Cloudinary
        public string OriginalFileName { get; set; }
        public long FileSizeBytes { get; set; }
        public int OrderIndex { get; set; }         // Thứ tự hiển thị của ảnh
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
