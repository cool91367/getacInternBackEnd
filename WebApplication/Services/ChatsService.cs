using WebApplication.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication.Services
{
    public class ChatsService
    {
        private readonly IMongoCollection<chat> chats;  //follow model

        public ChatsService(IChatDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            chats = database.GetCollection<chat>(settings.ChatsCollectionName);
        }

        public List<chat> Get() =>
            chats.Find(chat => true).ToList();

        public chat Get(string id) =>
            chats.Find<chat>(chat => chat.Id == id).FirstOrDefault();

        public string Create(chat chat)
        {
            chats.InsertOne(chat);
            string returnMessage = chat.Id.ToString() + " is stored in database";
            return returnMessage;
        }

        public void Update(string id, chat chatIn) =>
            chats.ReplaceOne(chat => chat.Id == id, chatIn);

        public void Remove(chat chatIn) =>
            chats.DeleteOne(chat => chat.Id == chatIn.Id);

        public void Remove(string id) =>
            chats.DeleteOne(chat => chat.Id == id);
    }
}