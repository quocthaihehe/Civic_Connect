using System;

namespace CivicConnect.Core.Entities
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
    }
}
