using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Statistics;
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

            var options = new GetInRangeOptions(key, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            var todayStatisticsCollection = await receiver.GetStatisticsInRangeAsync(options, timeSpan, cancellationToken);

            var mergedStatisticsCollection = MergeStatisticsCollections(statisticsInRangeCollection, todayStatisticsCollection);

            return Ok(mergedStatisticsCollection.Select(mapper.Map<LoadAmountStatisticsResponse>));
        }

        private static IEnumerable<LoadAmountStatistics> MergeStatisticsCollections(
            IEnumerable<LoadAmountStatistics> statisticsInRangeCollection, IEnumerable<LoadAmountStatistics> todayStatisticsCollection)
        {
            var mergedStatisticsCollection = statisticsInRangeCollection
                .Where(x => !todayStatisticsCollection.Any(y => x.DateFrom > y.DateFrom || x.DateTo > y.DateFrom)).ToList();
            mergedStatisticsCollection.AddRange(todayStatisticsCollection);

            return mergedStatisticsCollection;
        }
    }
}
