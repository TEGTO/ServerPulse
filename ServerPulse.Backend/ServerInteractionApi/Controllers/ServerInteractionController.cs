using EventCommunication.Events;
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

        [HttpPost("alive")]
        public async Task<IActionResult> SendAlive(AliveEvent aliveEvent, CancellationToken cancellationToken)
        {
            if (await serverSlotChecker.CheckSlotKeyAsync(aliveEvent.Key, cancellationToken))
            {
                await messageSender.SendAliveEventAsync(aliveEvent, cancellationToken);
                return Ok();
            }
            return NotFound($"Server slot with key '{aliveEvent.Key}' is not found!");
        }
        [HttpPost("configuration")]
        public async Task<IActionResult> SendConfiguration(ConfigurationEvent configurationEvent, CancellationToken cancellationToken)
        {
            if (await serverSlotChecker.CheckSlotKeyAsync(configurationEvent.Key, cancellationToken))
            {
                await messageSender.SendConfigurationEventAsync(configurationEvent, cancellationToken);
                return Ok();
            }
            return NotFound($"Server slot with key '{configurationEvent.Key}' is not found!");
        }
    }
}