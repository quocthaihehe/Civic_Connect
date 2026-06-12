using CivicConnect.Web.Models.Enums;
using System;
using System.Collections.Generic;

namespace CivicConnect.Web.Models.Entities
{
    public class Issue
    {
        public int Id { get; set; }
        public string Title { get; set; }             // Tiêu đề (5–200 ký tự)
        public string Description { get; set; }       // Mô tả chi tiết (20–5000 ký tự)
        public IssueCategory Category { get; set; }   // Loại phản ánh
        public IssueStatus Status { get; set; }       // Trạng thái xử lý
        public IssuePriority Priority { get; set; }   // Mức độ ưu tiên
        public float PriorityScore { get; set; }      // Điểm ưu tiên tự động (votes*0.5 + severity*0.3 + density*0.2)
        public float SeverityScore { get; set; } = 0.5f; // Điểm nghiêm trọng (0.0 - 1.0, mặc định 0.5)

        // Vị trí địa lý
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }           // Địa chỉ text (đầy đủ)
        public string WardCode { get; set; }          // Mã Phường/Xã
        public string WardName { get; set; }          // Tên Phường/Xã
        public string DistrictCode { get; set; }      // Mã Quận/Huyện
        public string DistrictName { get; set; }      // Tên Quận/Huyện
        public string ProvinceCode { get; set; }      // Mã Tỉnh/Thành phố
        public string ProvinceName { get; set; }      // Tên Tỉnh/Thành phố

        // Phân công xử lý
        public string? AssignedToUserId { get; set; } // Cán bộ xử lý
        public ApplicationUser? AssignedTo { get; set; }
        public string? AssignedUnitId { get; set; }    // Đơn vị cơ quan xử lý
        public GovernmentUnit? AssignedUnit { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? DueDate { get; set; }         // Hạn xử lý
        public DateTime? ResolvedAt { get; set; }

        // Metadata người gửi
        public string AuthorId { get; set; }          // Người gửi phản ánh
        public ApplicationUser Author { get; set; }
        public bool IsAnonymous { get; set; } = false;// Gửi ẩn danh
        public bool IsVerified { get; set; } = false; // Đã xác minh thực tế
        public int ViewCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Tự liên kết hỗ trợ luồng Chuyển cấp (Escalation)
        public int? ParentIssueId { get; set; }
        public Issue? ParentIssue { get; set; }
        public ICollection<Issue> ChildIssues { get; set; } = new List<Issue>();

        // Navigation
        public ICollection<IssueImage> Images { get; set; } = new List<IssueImage>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public ICollection<IssueStatusHistory> StatusHistory { get; set; } = new List<IssueStatusHistory>();
    }
}
