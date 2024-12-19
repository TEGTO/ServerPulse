using AnalyzerApi.Command.Controllers.GetSlotStatistics;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AnalyzerApi.Controllers
{
    [Route("slotdata")]
    [ApiController]
    public class SlotDataController : ControllerBase
    {
        private readonly IMediator mediator;

        public SlotDataController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [OutputCache(PolicyName = "GetSlotStatisticsPolicy")]
        [Route("{key}")]
        [HttpGet]
        public async Task<ActionResult<SlotStatisticsResponse>> GetSlotStatistics(string key, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetSlotStatisticsQuery(key), cancellationToken);
            return Ok(response);
        }
    }
}