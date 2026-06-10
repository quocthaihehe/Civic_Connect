using CivicConnect.Core.Entities;
using CivicConnect.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CivicConnect.Core.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUnreadAsync(string userId);
        Task SendNotificationAsync(string userId, string title, string message, NotificationType type, string? relatedIssueId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
    }
}
