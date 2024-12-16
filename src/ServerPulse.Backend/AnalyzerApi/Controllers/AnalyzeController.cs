using AnalyzerApi.Command.Controllers.GetAmountStatisticsInRange;
using AnalyzerApi.Command.Controllers.GetLoadEventsInDataRange;
using AnalyzerApi.Command.Controllers.GetSomeCustomEvents;
using AnalyzerApi.Command.Controllers.GetSomeLoadEvents;
using AnalyzerApi.Command.Controllers.GetWholeAmountStatisticsInDays;
using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AnalyzerApi.Controllers
{
    [Route("analyze")]
    [ApiController]
    public class AnalyzeController : ControllerBase
    {
        private readonly IMediator mediator;

        public AnalyzeController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [OutputCache(PolicyName = "GetLoadEventsInDataRangePolicy")]
        [Route("daterange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadEventResponse>>> GetLoadEventsInDataRange(MessagesInRangeRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetLoadEventsInDataRangeQuery(request), cancellationToken);
            return Ok(response);
        }

        [OutputCache(PolicyName = "GetDailyLoadStatisticsPolicy")]
        [Route("perday/{key}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetDailyLoadStatistics(string key, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetWholeAmountStatisticsInDaysQuery(key), cancellationToken);
            return Ok(response);
        }

        [OutputCache(PolicyName = "GetLoadAmountStatisticsInRangePolicy")]
        [Route("amountrange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetLoadAmountStatisticsInRange(MessageAmountInRangeRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetAmountStatisticsInRangeQuery(request), cancellationToken);
            return Ok(response);
        }

        [Route("someevents")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadEventResponse>>> GetSomeLoadEvents(GetSomeMessagesRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetSomeLoadEventsQuery(request), cancellationToken);
            return Ok(response);
        }

        [Route("somecustomevents")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<CustomEventResponse>>> GetSomeCustomEvents(GetSomeMessagesRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetSomeCustomEventsQuery(request), cancellationToken);
            return Ok(response);
        }
    }
}