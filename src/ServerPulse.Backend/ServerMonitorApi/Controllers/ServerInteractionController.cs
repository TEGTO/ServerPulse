using Microsoft.AspNetCore.Mvc;
using ServerMonitorApi.Services;
using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApi.Controllers
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

        [HttpPost("pulse")]
        public async Task<IActionResult> SendPulse(PulseEvent pulseEvent, CancellationToken cancellationToken)
        {
            if (await serverSlotChecker.CheckSlotKeyAsync(pulseEvent.Key, cancellationToken))
            {
                await messageSender.SendPulseEventAsync(pulseEvent, cancellationToken);
                return Ok();
            }
            return NotFound($"Server slot with key '{pulseEvent.Key}' is not found!");
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