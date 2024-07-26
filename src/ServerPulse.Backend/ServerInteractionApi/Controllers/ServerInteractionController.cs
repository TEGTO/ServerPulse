using Microsoft.AspNetCore.Mvc;
using ServerInteractionApi.Services;
using ServerPulse.EventCommunication.Events;

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
        [HttpPost("load")]
        public async Task<IActionResult> SendLoadEvents(LoadEvent[] loadEvents, CancellationToken cancellationToken)
        {
            var firstKey = loadEvents.First().Key;
            if (!loadEvents.All(x => x.Key == firstKey))
            {
                return BadRequest($"All load events must have the same key per request!");
            }

            if (await serverSlotChecker.CheckSlotKeyAsync(firstKey, cancellationToken))
            {
                await messageSender.SendLoadEventsAsync(loadEvents, cancellationToken);
                return Ok();
            }
            return NotFound($"Server slot with key '{firstKey}' is not found!");
        }
    }
}