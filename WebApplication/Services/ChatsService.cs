using WebApplication.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication.Services
{
    public class ChatsService
    {
        private readonly IMongoCollection<Chat> chats;

        public ChatsService(IChatDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            chats = database.GetCollection<Chat>(settings.ChatsCollectionName);
        }

        public List<Chat> Get() =>
            chats.Find(chat => true).ToList();

        public Chat Get(string id) =>
            chats.Find<Chat>(chat => chat.Id == id).FirstOrDefault();

        public Chat GetChatByTopic(string topic)
        {
            var chatBase = chats.Find<Chat>(chat => chat.Topic == topic).ToList().First();
            return chatBase;

        }

        public List<string> GetTopics()
        {
            List<string> topics = new List<string>();
            foreach (Chat chat in Get()) topics.Add(chat.Topic);
            return topics;
        }

        public string Create(Chat chat)
        {
            chats.InsertOne(chat);
            string returnMessage = chat.Id.ToString() + " is stored in database";
            return returnMessage;
        }

        public void Update(string id, Chat chatIn) =>
            chats.ReplaceOne(chat => chat.Id == id, chatIn);

        public void Remove(Chat chatIn) =>
            chats.DeleteOne(chat => chat.Id == chatIn.Id);

        public void Remove(string id) =>
            chats.DeleteOne(chat => chat.Id == id);
    }
}