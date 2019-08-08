using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Confluent.Kafka;
using WebApplication.Models;
using WebApplication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
