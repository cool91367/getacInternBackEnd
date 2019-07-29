using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace WebApplication.Models
{
    public class chat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Topic")]
        [JsonProperty("Topic")]
        public string Topic { get; set; }

        public string Content { get; set; }

    }
}