using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CivicConnect.Web.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
