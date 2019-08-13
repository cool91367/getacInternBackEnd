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
                await Clients.All.SendAsync("ReceiveMessage", message);
            }
        }

         public async Task UploadMessage(List<Chat> chats)
        {
            var config = new ProducerConfig { BootstrapServers = Constant.BrokerIP };
            foreach (Chat chat in chats)
            {
                using (var producer = new ProducerBuilder<string, string>(config).Build())
                {
                    try
                    {
                        var deliveryReport = await producer.ProduceAsync(
                            chat.Topic, new Message<string, string> { Key = chat.Id, Value = JsonConvert.SerializeObject(chat.Content)});
                    }
                    catch (ProduceException<string, string> e)
                    {
                        Console.WriteLine($"failed to deliver message: {e.Message} [{e.Error.Code}]");
                    }
                }
            }
        }
    }
}