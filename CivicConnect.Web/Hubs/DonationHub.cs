using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CivicConnect.Web.Hubs
{
    public class DonationHub : Hub
    {
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, orderId);
        }

        public async Task LeaveOrderGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, orderId);
        }
    }
}
