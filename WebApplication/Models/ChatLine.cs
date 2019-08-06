using System;
namespace WebApplication.Models
{
    public class ChatLine
    {
        public ChatLine() { }

        public ChatLine(string chatString, string senderId)
        {
            this.ChatString = chatString;
            this.SenderId = senderId;
        }

        public string ChatString { get; set; }

        public DateTime SendTime { get; set; } = DateTime.Now;

        public string SenderId { get; set; }
    }
}