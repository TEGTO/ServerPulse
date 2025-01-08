using AnalyzerApi.Infrastructure.Dtos.Endpoints.Analyze.GetSomeLoadEvents;
using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AnalyzerApi.Endpoints.Analyze.GetSomeLoadEvents
{
    [Route("analyze")]
    [ApiController]
    public class GetSomeLoadEventsController : ControllerBase
    {
        private readonly IEventReceiver<LoadEventWrapper> receiver;
        private readonly IMapper mapper;

        public GetSomeLoadEventsController(IEventReceiver<LoadEventWrapper> receiver, IMapper mapper)
        {
            this.receiver = receiver;
            this.mapper = mapper;
        }

        [Route("someevents")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadEventResponse>>> GetSomeLoadEvents(GetSomeLoadEventsRequest request, CancellationToken cancellationToken)
        {
            var options = new GetCertainMessageNumberOptions(request.Key, request.NumberOfMessages, request.StartDate.ToUniversalTime(), request.ReadNew);
            var events = await receiver.GetCertainAmountOfEventsAsync(options, cancellationToken);

            return Ok(events.Select(mapper.Map<LoadEventResponse>));
        }
    }
}
