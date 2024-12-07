using AnalyzerApi.Command.ProcessLoadEvent;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Controllers
{
    [Route("eventprocessing")]
    [ApiController]
    public class EventProcessingController : ControllerBase
    {
        private readonly IMediator mediator;

        public EventProcessingController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [Route("load")]
        [HttpPost]
        public async Task<IActionResult> ProcessLoadEvents(LoadEvent[] events, CancellationToken cancellationToken)
        {
            await mediator.Send(new ProcessLoadEventsCommand(events), cancellationToken);
            return Ok();
        }
    }
}