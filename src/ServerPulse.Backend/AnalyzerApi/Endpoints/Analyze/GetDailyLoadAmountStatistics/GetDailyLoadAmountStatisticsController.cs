using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Services.Receivers.Statistics;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

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
        [Route("perday/{key}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetDailyLoadAmountStatistics(string key, CancellationToken cancellationToken)
        {
            var timeSpan = TimeSpan.FromDays(1);

            var statisticsInRangeCollection = await receiver.GetWholeStatisticsInTimeSpanAsync(key, timeSpan, cancellationToken);

            return Ok(statisticsInRangeCollection.Select(mapper.Map<LoadAmountStatisticsResponse>));
        }
    }
}
