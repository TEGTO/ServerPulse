using AnalyzerApi.Application.Services.Receivers.Event;
using AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetSomeCustomEvents;
using AnalyzerApi.Core.Dtos.Responses.Events;
using AnalyzerApi.Core.Models;
using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<IEnumerable<CustomEventResponse>>> GetSomeCustomEvents(
            GetSomeCustomEventsRequest request, CancellationToken cancellationToken)
        {
            var options = new GetCertainMessageNumberOptions(request.Key, request.NumberOfMessages, request.StartDate.ToUniversalTime(), request.ReadNew);
            var events = await receiver.GetCertainAmountOfEventsAsync(options, cancellationToken);

            return Ok(events.Select(mapper.Map<CustomEventResponse>));
        }
    }
}
