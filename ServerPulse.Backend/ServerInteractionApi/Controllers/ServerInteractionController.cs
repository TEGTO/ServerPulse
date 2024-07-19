using Microsoft.AspNetCore.Mvc;
using ServerInteractionApi.Services;

namespace ServerInteractionApi.Controllers
{
    [Route("serverinteraction")]
    [ApiController]
    public class ServerInteractionController : ControllerBase
    {
        private readonly IMessageSender messageSender;

        public ServerInteractionController(IMessageSender messageSender)
        {
            this.messageSender = messageSender;
        }

        [HttpPost("alive/{id}")]
        public async Task<IActionResult> SendAlive(string id, CancellationToken cancellationToken)
        {
            await messageSender.SendAliveEventAsync(id);
            return Ok();
        }
    }
}