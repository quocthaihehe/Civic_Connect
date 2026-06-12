using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CivicConnect.Web.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUnreadAsync(string userId);
        Task SendNotificationAsync(string userId, string title, string message, NotificationType type, string? relatedIssueId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
    }
}
