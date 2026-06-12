using System;

namespace CivicConnect.Web.Models.Entities
{
    public class Policy
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Tag { get; set; } = "Thông báo";
        public string TagClass { get; set; } = "tag-notice";
        public string IssuingUnit { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Các thuộc tính chứng thực pháp lý mới
        public string? DocumentNumber { get; set; }
        public string? DocumentType { get; set; }
        public string? Signer { get; set; }
        public string? SourceUrl { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }
}
