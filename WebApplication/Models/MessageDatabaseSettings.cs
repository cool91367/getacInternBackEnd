namespace WebApplication.Models
{
    public class ChatDatabaseSettings : IChatDatabaseSettings
    {
        public string ChatsCollectionName { get; set; }

        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }
    }

    public interface IChatDatabaseSettings
    {
        string ChatsCollectionName { get; set; }

        string ConnectionString { get; set; }

        string DatabaseName { get; set; }
    }
}