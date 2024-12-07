using AnalyzerApi.Command.GetAmountStatisticsInRange;
using AnalyzerApi.Command.GetLoadEventsInDataRange;
using AnalyzerApi.Command.GetSomeCustomEvents;
using AnalyzerApi.Command.GetSomeLoadEvents;
using AnalyzerApi.Command.GetWholeAmountStatisticsInDays;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Requests;
using AnalyzerApi.Infrastructure.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AnalyzerApi.Controllers
{
    [Route("analyze")]
    [ApiController]
    public class AnalyzeController : ControllerBase
    {
        #region Fields
        private readonly IMediator mediator;

        #endregion

        public AnalyzeController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        #region Endpoints

        [OutputCache(PolicyName = "GetLoadEventsInDataRangePolicy")]
        [Route("daterange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadEventWrapper>>> GetLoadEventsInDataRange(MessagesInRangeRangeRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetLoadEventsInDataRangeQuery(request), cancellationToken);
            return Ok(response);
        }

        [OutputCache(PolicyName = "GetWholeAmountStatisticsInDaysPolicy")]
        [Route("perday/{key}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetWholeAmountStatisticsInDays(string key, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetWholeAmountStatisticsInDaysQuery(key), cancellationToken);
            return Ok(response);
        }

        [OutputCache(PolicyName = "GetAmountStatisticsInRangePolicy")]
        [Route("amountrange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetAmountStatisticsInRange(MessageAmountInRangeRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetAmountStatisticsInRangeQuery(request), cancellationToken);
            return Ok(response);
        }

        [Route("someevents")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadEventWrapper>>> GetSomeLoadEvents(GetSomeMessagesRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetSomeLoadEventsQuery(request), cancellationToken);
            return Ok(response);
        }

        [Route("somecustomevents")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<CustomEventWrapper>>> GetSomeCustomEvents(GetSomeMessagesRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetSomeCustomEventsQuery(request), cancellationToken);
            return Ok(response);
        }

        #endregion
    }
}