using Microsoft.AspNetCore.Mvc;
using ServerMonitorApi.Services;
using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApi.Controllers
{
    [Route("serverinteraction")]
    [ApiController]
    public class ServerInteractionController : ControllerBase
    {
        private readonly IEventSender eventSender;
        private readonly ISlotKeyChecker serverSlotChecker;
        private readonly IEventProcessing eventProcessing;

        public ServerInteractionController(IEventSender eventSender, ISlotKeyChecker serverSlotChecker, IEventProcessing eventProcessing)
        {
            this.eventSender = eventSender;
            this.serverSlotChecker = serverSlotChecker;
            this.eventProcessing = eventProcessing;
        }

        [HttpPost("pulse")]
        public async Task<IActionResult> SendPulse(PulseEvent pulseEvent, CancellationToken cancellationToken)
        {
            if (await serverSlotChecker.CheckSlotKeyAsync(pulseEvent.Key, cancellationToken))
            {
                await eventSender.SendEventsAsync(new[] { pulseEvent }, cancellationToken);
                return Ok();
            }
            return NotFound($"Server slot with key '{pulseEvent.Key}' is not found!");
        }
        [HttpPost("configuration")]
        public async Task<IActionResult> SendConfiguration(ConfigurationEvent configurationEvent, CancellationToken cancellationToken)
        {
            if (await serverSlotChecker.CheckSlotKeyAsync(configurationEvent.Key, cancellationToken))
            {
                await eventSender.SendEventsAsync(new[] { configurationEvent }, cancellationToken);
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
                await eventProcessing.SendEventsForProcessingsAsync(loadEvents, cancellationToken);
                await eventSender.SendEventsAsync(loadEvents, cancellationToken);
                return Ok();
            }
            return NotFound($"Server slot with key '{firstKey}' is not found!");
        }
    }
}