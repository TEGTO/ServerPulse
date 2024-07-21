using Microsoft.AspNetCore.Mvc;
using ServerInteractionApi.Services;

namespace ServerInteractionApi.Controllers
{
    [Route("serverinteraction")]
    [ApiController]
    public class ServerInteractionController : ControllerBase
    {
        private readonly IMessageSender messageSender;
        private readonly ISlotKeyChecker serverSlotChecker;

        public ServerInteractionController(IMessageSender messageSender, ISlotKeyChecker serverSlotChecker)
        {
            this.messageSender = messageSender;
            this.serverSlotChecker = serverSlotChecker;
        }

        [HttpPost("alive/{slotKey}")]
        public async Task<IActionResult> SendAlive(string slotKey, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(slotKey))
            {
                throw new ArgumentException("Key must be provided");
            }

            if (await serverSlotChecker.CheckSlotKeyAsync(slotKey, cancellationToken))
            {
                await messageSender.SendAliveEventAsync(slotKey, cancellationToken);
                return Ok();
            }
            return NotFound($"Server slot with key '{slotKey}' is not found!");
        }
    }
}