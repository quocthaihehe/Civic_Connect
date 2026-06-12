using CivicConnect.Web.Models.Enums;
using System;

namespace CivicConnect.Web.Models.Entities
{
    public class IssueStatusHistory
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public Issue Issue { get; set; }

        public IssueStatus FromStatus { get; set; }
        public IssueStatus ToStatus { get; set; }

        public string ChangedById { get; set; }      // Cán bộ thực hiện thay đổi
        public ApplicationUser ChangedBy { get; set; }

        public string? Note { get; set; }            // Ghi chú lý do chuyển trạng thái
        public string? AttachmentUrl { get; set; }  // URL văn bản, tài liệu đính kèm (nếu có)
        
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
