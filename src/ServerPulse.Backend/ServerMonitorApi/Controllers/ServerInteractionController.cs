using EventCommunication;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServerMonitorApi.Command.SendConfiguration;
using ServerMonitorApi.Command.SendCustomEvents;
using ServerMonitorApi.Command.SendLoadEvents;
using ServerMonitorApi.Command.SendPulse;

namespace ServerMonitorApi.Controllers
{
    [Route("serverinteraction")]
    [ApiController]
    public class ServerInteractionController : ControllerBase
    {
        private readonly IMediator mediator;

        public ServerInteractionController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost("pulse")]
        public async Task<IActionResult> SendPulse(PulseEvent pulseEvent, CancellationToken cancellationToken)
        {
            await mediator.Send(new SendPulseCommand(pulseEvent), cancellationToken);
            return Ok();
        }

        [HttpPost("configuration")]
        public async Task<IActionResult> SendConfiguration(ConfigurationEvent configurationEvent, CancellationToken cancellationToken)
        {
            await mediator.Send(new SendConfigurationCommand(configurationEvent), cancellationToken);
            return Ok();
        }

        [HttpPost("load")]
        public async Task<IActionResult> SendLoadEvents(LoadEvent[] loadEvents, CancellationToken cancellationToken)
        {
            await mediator.Send(new SendLoadEventsCommand(loadEvents), cancellationToken);
            return Ok();
        }

        [HttpPost("custom")]
        public async Task<IActionResult> SendCustomEvents(CustomEventContainer[] customEventWrappers, CancellationToken cancellationToken)
        {
            await mediator.Send(new SendCustomEventsCommand(customEventWrappers), cancellationToken);
            return Ok();
        }
    }
}