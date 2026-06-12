using CivicConnect.Web.Models.Enums;
using System;

namespace CivicConnect.Web.Models.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public string? RelatedIssueId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
