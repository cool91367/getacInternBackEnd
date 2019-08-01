using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebApplication.Models;
using WebApplication.Services;
namespace WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {

        private readonly ChatsService chatsService;

        public ChatsController(ChatsService chatsService)
        {
            this.chatsService = chatsService;
        }

        [HttpGet]
        public ActionResult<List<Chat>> Get() =>
            chatsService.Get();

        [HttpGet("{id:length(24)}", Name = "GetChat")]
        public ActionResult<Chat> Get(string id)
        {
            var chat = chatsService.Get(id);

            if (chat == null)
            {
                return NotFound();
            }

            return chat;
        }

        [HttpGet()]
        [Route("GetChatByTopic/{topic}")]
        public ActionResult<Chat> GetChatByTopic(string topic)
        {
            var chat = chatsService.GetChatByTopic(topic);
            if (chat == null) return NotFound();
            return chat;
        }

        [HttpGet()]
        [Route("GetAllTopicInDB")]
        public List<string> GetAllTopicInDB()
        {
            return chatsService.GetTopics();
        }

        [HttpPost]
        public ActionResult<Chat> Create(Chat chat)
        {
            chatsService.Create(chat);

            return CreatedAtRoute("GetChat", new { id = chat.Id.ToString() }, chat);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Chat ChatIn)
        {
            var chat = chatsService.Get(id);

            if (chat == null)
            {
                return NotFound();
            }

            chatsService.Update(id, ChatIn);
            chat = chatsService.Get(id);

            return CreatedAtRoute("GetChat", chat);
        }

        [HttpPut]
        [Route("UpdateByTopic/{topic}")]
        public IActionResult UpdateByTopic(string topic, [FromBody] Chat chat)
        {
            if (chat == null) return NotFound();
            var result = chatsService.UpdateByTopic(topic, chat);
            if (result != null) return Ok(result);
            return NotFound(chat);
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var chat = chatsService.Get(id);

            if (chat == null)
            {
                return NotFound();
            }

            chatsService.Remove(chat.Id);

            return NoContent();
        }
    }
}