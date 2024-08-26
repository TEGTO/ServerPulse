using AnalyzerApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Controllers
{
    [Route("eventprocessing")]
    [ApiController]
    public class EventProcessingController : ControllerBase
    {
        private readonly IEventProcessor eventProcessor;

        public EventProcessingController(IEventProcessor eventProcessor)
        {
            this.eventProcessor = eventProcessor;
        }

        [Route("load")]
        [HttpPost]
        public async Task<IActionResult> ProcessLoadEvent([FromBody] LoadEvent[] events, CancellationToken cancellationToken)
        {
            if (events == null || events.Length == 0 || !events.All(x => x.Key == events.First().Key))
            {
                return BadRequest();
            }

            await eventProcessor.ProcessEventsAsync(events, cancellationToken);
            return Ok();
        }
    }
}