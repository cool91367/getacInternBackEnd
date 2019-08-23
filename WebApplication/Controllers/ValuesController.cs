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
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Text;

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


        [AllowAnonymous]
        [HttpGet("getPictureName")]
        public ActionResult<List<string>> getPictureName()
        {
            string PicFolder = "B3Image\\";
            List<string> picturesName = new List<string>();
            foreach (string Picture in Directory.GetFiles(PicFolder))
            {
                var pictureName = Picture.Replace(PicFolder, "");
                //var pictureName = Picture.Split("\")[1];
                picturesName.Add(pictureName);
            }
            return picturesName;
        }

        [AllowAnonymous]
        [HttpGet("getPicture/{name}")]
        public ActionResult<List<string>> getPicture(string name)
        {
            //List<string> picturesName = new List<string>();
            string PicPath = "B3Image/" + name;
            FileStream fileStream = new FileStream(PicPath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            byte[] byteData = binaryReader.ReadBytes((int)fileStream.Length);
            List<string> picturesName = new List<string>();
            picturesName.Add(Convert.ToBase64String(byteData));
            return picturesName;
        }

        [AllowAnonymous]
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

            using (var consumer = new ConsumerBuilder<Ignore, byte[]>(config).Build())
            {
                string fileName = "";
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
                            Console.WriteLine(cr.Topic);
                            // store the image
                            if (cr.Topic.Equals("FileContent"))
                            {
                                if (fileName != "")
                                {
                                    Console.WriteLine(cr.Value);
                                    FileStream myFile = System.IO.File.Open(@"B3Image/" + fileName, FileMode.Create, FileAccess.Write);
                                    BinaryWriter myWriter = new BinaryWriter(myFile);
                                    myWriter.Write(cr.Value);
                                    myWriter.Close();
                                    myFile.Close();
                                    fileName = "";
                                }
                            }
                            else if (cr.Topic.Equals("FileName"))
                            {
                                fileName = Encoding.Default.GetString(cr.Value);
                            }
                            else
                            {
                                var incomming = new Chat();
                                try
                                {
                                    ChatLine chatPackage = JsonConvert.DeserializeObject<List<ChatLine>>(Encoding.Default.GetString(cr.Value)).First();
                                    incomming = new Chat(cr.Topic, chatPackage);
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("Deserialize failed, check MongoDB if it is delivered from CLI.");
                                    ChatLine chatPackage = new ChatLine(Encoding.Default.GetString(cr.Value), "CLTestUser");
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
