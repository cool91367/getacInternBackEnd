using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string message)
        {
            if (Clients != null)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", message);
            }
        }
        
    }
}
