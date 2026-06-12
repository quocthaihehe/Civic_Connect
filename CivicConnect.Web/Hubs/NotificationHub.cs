using CivicConnect.Web.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CivicConnect.Web.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly AppDbContext _context;

        public NotificationHub(AppDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                userId = Context.UserIdentifier;
            }

            if (!string.IsNullOrEmpty(userId))
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new {
                        id = n.Id,
                        title = n.Title,
                        message = n.Message,
                        type = n.Type,
                        relatedIssueId = n.RelatedIssueId,
                        isRead = n.IsRead,
                        createdAt = n.CreatedAt
                    })
                    .ToListAsync();

                await Clients.Caller.SendAsync("LoadNotifications", notifications);
            }

            await base.OnConnectedAsync();
        }
    }
}
