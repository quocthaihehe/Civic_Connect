using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using CivicConnect.Web.Repositories;
using CivicConnect.Web.Data;
using CivicConnect.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CivicConnect.Web.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<Notification>> GetUnreadAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task SendNotificationAsync(string userId, string title, string message, NotificationType type, string? relatedIssueId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedIssueId = relatedIssueId
            };
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            // Đẩy thông báo thời gian thực qua SignalR
            await _hubContext.Clients.User(userId).SendAsync("IssueStatusChanged", new
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                relatedIssueId = notification.RelatedIssueId
            });
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
            {
                n.IsRead = true;
            }
            await _context.SaveChangesAsync();
        }
    }
}

