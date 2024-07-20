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

        [HttpPost("alive/{id}")]
        public async Task<IActionResult> SendAlive(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Id must be provided");
            }

            if (await serverSlotChecker.CheckServerSlotAsync(id, cancellationToken))
            {
                await messageSender.SendAliveEventAsync(id);
                return Ok();
            }
            return NotFound($"Server slot with {id} id is not found!");
        }
    }
}