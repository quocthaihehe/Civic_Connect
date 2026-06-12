using System;
using System.Collections.Generic;
using CivicConnect.Web.Models.Enums;

namespace CivicConnect.Web.Models.Home
{
    public class HomeViewModel
    {
        // User info
        public string UserFullName { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public int MyPendingCount { get; set; }
        
        // Greeting theo giờ
        public string TimeGreeting => DateTime.Now.Hour switch
        {
            < 12 => "Chào buổi sáng",
            < 18 => "Chào buổi chiều",
            _    => "Chào buổi tối"
        };
        
        // Section 3: Tin tức chính sách
        public List<PolicySummaryDto> LatestPolicies { get; set; } = new();
        
        // Section 4: Phản ánh nổi bật đã duyệt
        public List<IssueFeedDto> FeaturedIssues { get; set; } = new();
        
        // Section 5: Phản ánh của tôi
        public List<MyIssueRowDto> MyRecentIssues { get; set; } = new();
    }

    public class PolicySummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;         // "Luật mới", "Thông báo", "Chính sách"
        public string TagClass { get; set; } = string.Empty;    // "tag-law", "tag-notice", "tag-policy"
        public string IssuingUnit { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
    }

    public class IssueFeedDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string Address { get; set; } = string.Empty;
        public string CategoryLabel { get; set; } = string.Empty;
        public string CategoryClass { get; set; } = string.Empty;  // "cat-traffic", "cat-environment"...
        public string StatusLabel { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;    // "pending", "processing", "resolved"
        public int VoteCount { get; set; }
        public int CommentCount { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorAvatar { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class MyIssueRowDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryLabel { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public IssueStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.Now && Status != IssueStatus.Resolved;
        public int DaysLeft => DueDate.HasValue ? (int)Math.Max(0, (DueDate.Value - DateTime.Now).TotalDays) : 0;
    }
}
