using AnalyzerApi.Application.Services.Receivers.Statistics;
using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AutoMapper;
using ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Swashbuckle.AspNetCore.Annotations;

namespace AnalyzerApi.Endpoints.Analyze.GetDailyLoadAmountStatistics
{
    [Route("analyze")]
    [ApiController]
    public class GetDailyLoadAmountStatisticsController : ControllerBase
    {
        private readonly ILoadAmountStatisticsReceiver receiver;
        private readonly IMapper mapper;

        public GetDailyLoadAmountStatisticsController(ILoadAmountStatisticsReceiver receiver, IMapper mapper)
        {
            this.receiver = receiver;
            this.mapper = mapper;
        }

        [OutputCache(PolicyName = "GetDailyLoadAmountStatisticsPolicy")]
        [HttpGet("perday/{key}")]
        [SwaggerOperation(
            Summary = "Retrieve daily load event amount.",
            Description = "Fetches the number of load events recorded each day for the past year."
        )]
        [ProducesResponseType(typeof(IEnumerable<LoadAmountStatisticsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetDailyLoadAmountStatistics(string key, CancellationToken cancellationToken)
        {
            var timeSpan = TimeSpan.FromDays(1);

            var statisticsInRangeCollection = await receiver.GetWholeStatisticsInTimeSpanAsync(key, timeSpan, cancellationToken);

            return Ok(statisticsInRangeCollection.Select(mapper.Map<LoadAmountStatisticsResponse>));
        }
    }
}
