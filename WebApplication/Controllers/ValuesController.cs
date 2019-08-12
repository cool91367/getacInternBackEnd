using System.Linq;
using Confluent.Kafka;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using SignalRChat.Hubs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApplication.Kafka;
using WebApplication.Models;
using WebApplication.Services;
using  Newtonsoft.Json.Serialization;

namespace WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private ChatHub chatHub;

        private ChatsService chatsService;


        public ValuesController(ChatHub chatHub, ChatsService chatsService)
        {
            this.chatHub = chatHub;
            this.chatsService = chatsService;
        }

        [HttpGet("KafkaConsumer")]
        public async Task KafkaConsumer()
        {
            List<string> topicListFromDB = chatsService.GetTopics();
            ConsumerConfig config = JsonConvert.DeserializeObject<ConsumerConfig>(System.IO.File.ReadAllText(Constant.ConsumerConfigFilePath));
            var topicListFromServer = AdminClient.RemoveAdminTopics(AdminClient.getMetadata(Constant.BrokerIP));
            foreach (string topic in topicListFromServer)
            {
                if (!topicListFromDB.Contains(topic))
                {
                    Chat chat = new Chat(topic);
                    chatsService.Create(chat);
                }
            }

            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe(topicListFromServer);
                CancellationTokenSource cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true; // prevent the process from terminating.
                    cts.Cancel();
                };

                try
                {
                    while (true)
                    {
                        try
                        {
                            var cr = consumer.Consume(cts.Token);
                            var incomming = new Chat();
                            try
                            {
                                ChatLine chatPackage = JsonConvert.DeserializeObject<List<ChatLine>>(cr.Value).First();
                                incomming = new Chat(cr.Topic, chatPackage);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Deserialize failed, check MongoDB if it is delivered from CLI.");
                                ChatLine chatPackage = new ChatLine(cr.Value, "CLTestUser");
                                incomming = new Chat(cr.Topic, chatPackage);
                            }

                            if (topicListFromDB.Contains(cr.Topic))
                            {
                                //Add new line to DB
                                chatsService.UpdateByTopic(cr.Topic, incomming);
                            }
                            else
                            {
                                //create new Topic
                                chatsService.Create(incomming);
                                topicListFromDB.Add(cr.Topic);
                            }

                            var allChats = JsonConvert.SerializeObject
                            (
                                chatsService.Get(),
                                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
                            );
                            await chatHub.SendMessage(allChats);

                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Error occured: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ensure the consumer leaves the group cleanly and final offsets are committed.
                    consumer.Close();
                }
            }
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            List<string> output = new List<string>();
            output.Add("use https://localhost:5001/api/values/KafkaConsumer to init consumer thread.");
            return output;
        }
    }
}
