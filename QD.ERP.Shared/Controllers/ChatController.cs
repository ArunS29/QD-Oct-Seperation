using Microsoft.AspNetCore.Mvc;
using QD.ERP.Shared.Service; // ✅ Correct your actual namespace here
using System.Text;
using System.Threading.Tasks;

namespace QD.ERP.Shared.Controllers
{
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly OpenAIChatService _chatService;

        public ChatController(OpenAIChatService chatService)
        {
            _chatService = chatService;
        }

        public class ChatRequest
        {
            public string Message { get; set; }
        }

        // ✅ STREAMING endpoint only
        [HttpPost("stream")]
        public async Task Stream([FromBody] ChatRequest input)
        {
            Response.ContentType = "application/octet-stream";
            var sessionId = HttpContext.Session.Id;

            await _chatService.StreamMessageAsync(sessionId, input.Message, async chunk =>
            {
                var buffer = Encoding.UTF8.GetBytes(chunk);
                await Response.Body.WriteAsync(buffer, 0, buffer.Length);
                await Response.Body.FlushAsync();
            });
        }
    }
}
