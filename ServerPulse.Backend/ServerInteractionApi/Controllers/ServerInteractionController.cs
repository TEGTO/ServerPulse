using Microsoft.AspNetCore.Mvc;
using ServerInteractionApi.Services;

namespace ServerInteractionApi.Controllers
{
    [Route("serverinteraction")]
    [ApiController]
    public class ServerInteractionController : ControllerBase
    {
        private readonly IMessageSender messageSender;
        private readonly IServerSlotChecker serverSlotChecker;

        public ServerInteractionController(IMessageSender messageSender, IServerSlotChecker serverSlotChecker)
        {
            this.messageSender = messageSender;
            this.serverSlotChecker = serverSlotChecker;
        }

        [HttpPost("alive/{key}")]
        public async Task<IActionResult> SendAlive(string key, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key must be provided");
            }

            if (await serverSlotChecker.CheckServerSlotAsync(key, cancellationToken))
            {
                await messageSender.SendAliveEventAsync(key);
                return Ok();
            }
            return NotFound($"Server slot with key '{key}' is not found!");
        }
    }
}