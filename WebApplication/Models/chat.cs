using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
namespace WebApplication.Models
{
    public class Chat
    {
        public Chat() {}

        public Chat(string topic, ChatLine singleLine)
        {
            this.Topic = topic;
            this.Content = new List<ChatLine>();
            this.Content.Add(singleLine);
        }

        public Chat(string topic)
        {
            this.Topic = topic;
            this.Content = new List<ChatLine>();
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Topic")]
        [JsonProperty("Topic")]
        public string Topic { get; set; }

        public List<ChatLine> Content { get; set; }
    }
}