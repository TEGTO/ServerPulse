using AnalyzerApi.Domain.Dtos;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ServerMonitorApi.Services;
using Shared;
using System.Text.Json;

namespace AnalyzerApi.Controllers
{
    [Route("analyze")]
    [ApiController]
    public class AnalyzeController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IServerLoadReceiver serverLoadReceiver;
        private readonly ICacheService cacheService;
        private readonly double cacheExpiryInMinutes;
        private readonly string cacheStatisticsKey;

        public AnalyzeController(IMapper mapper, IServerLoadReceiver serverLoadReceiver, ICacheService cacheService, IConfiguration configuration)
        {
            this.mapper = mapper;
            this.serverLoadReceiver = serverLoadReceiver;
            this.cacheService = cacheService;
            cacheExpiryInMinutes = double.Parse(configuration[Configuration.CACHE_SERVER_LOAD_STATISTICS_PER_DAY_EXPIRY_IN_MINUTES]!);
            cacheStatisticsKey = configuration[Configuration.CACHE_STATISTICS_KEY]!;
        }

        [Route("daterange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<ServerLoadResponse>>> GetLoadEventsInDataRange([FromBody] LoadEventsRangeRequest request, CancellationToken cancellationToken)
        {
            var options = new InRangeQueryOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
            var events = await serverLoadReceiver.ReceiveEventsInRangeAsync(options, cancellationToken);
            return Ok(events.Select(mapper.Map<ServerLoadResponse>));
        }
        [Route("perday/{key}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetWholeAmountStatisticsInDays(string key, CancellationToken cancellationToken)
        {
            var cacheKey = $"{cacheStatisticsKey}{key}-perday";

            List<LoadAmountStatistics>? statistics = await GetStatisticsInCacheAsync(cacheKey);

            if (statistics == null)
            {
                statistics = (await serverLoadReceiver.GetAmountStatisticsInDaysAsync(key, cancellationToken)).ToList();
            }
            var todayStatistics = await serverLoadReceiver.GetAmountStatisticsLastDayAsync(key, cancellationToken);

            var response = statistics.Where(x => !todayStatistics.Any(y => x.Date == y.Date)).ToList();
            response.AddRange(todayStatistics);

            await cacheService.SetValueAsync(cacheKey, JsonSerializer.Serialize(response), cacheExpiryInMinutes);

            return Ok(response.Select(mapper.Map<LoadAmountStatisticsResponse>));
        }
        [Route("amountrange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetAmountStatisticsInRange([FromBody] LoadAmountStatisticsInRangeRequest request, CancellationToken cancellationToken)
        {
            var options = new InRangeQueryOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
            var statistics = await serverLoadReceiver.GetAmountStatisticsInRangeAsync(options, request.TimeSpan, cancellationToken);

            return Ok(statistics.Select(mapper.Map<LoadAmountStatisticsResponse>));
        }

        private async Task<List<LoadAmountStatistics>?> GetStatisticsInCacheAsync(string key)
        {
            var json = await cacheService.GetValueAsync(key);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            if (json.TryToDeserialize(out List<LoadAmountStatistics> response))
            {
                return response;
            }
            return null;
        }
    }
}