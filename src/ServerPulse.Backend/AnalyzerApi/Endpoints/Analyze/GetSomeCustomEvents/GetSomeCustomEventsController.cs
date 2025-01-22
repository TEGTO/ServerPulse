using AnalyzerApi.Application.Services.Receivers.Event;
using AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetSomeCustomEvents;
using AnalyzerApi.Core.Dtos.Responses.Events;
using AnalyzerApi.Core.Models;
using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AnalyzerApi.Endpoints.Analyze.GetSomeCustomEvents
{
    [Route("analyze")]
    [ApiController]
    public class GetSomeCustomEventsController : ControllerBase
    {
        private readonly IEventReceiver<CustomEventWrapper> receiver;
        private readonly IMapper mapper;

        public GetSomeCustomEventsController(IEventReceiver<CustomEventWrapper> receiver, IMapper mapper)
        {
            this.receiver = receiver;
            this.mapper = mapper;
        }

        [Route("somecustomevents")]
        [HttpPost]
        [SwaggerOperation(
            Summary = "Retrieve custom events in date range.",
            Description = "Fetches the custom events recorded in date range."
        )]
        [ProducesResponseType(typeof(IEnumerable<CustomEventResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CustomEventResponse>>> GetSomeCustomEvents(
            GetSomeCustomEventsRequest request, CancellationToken cancellationToken)
        {
            var options = new GetCertainMessageNumberOptions(request.Key, request.NumberOfMessages, request.StartDate.ToUniversalTime(), request.ReadNew);
            var events = await receiver.GetCertainAmountOfEventsAsync(options, cancellationToken);

            return Ok(events.Select(mapper.Map<CustomEventResponse>));
        }
    }
}
