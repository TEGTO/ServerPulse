using AnalyzerApi.Domain.Dtos.Requests;
using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
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
        private readonly ICustomReceiver customReceiver;
        private readonly ICacheService cacheService;
        private readonly double cacheExpiryInMinutes;
        private readonly string cacheStatisticsKey;

        public AnalyzeController(IMapper mapper, IServerLoadReceiver serverLoadReceiver, ICustomReceiver customReceiver, ICacheService cacheService, IConfiguration configuration)
        {
            this.mapper = mapper;
            this.serverLoadReceiver = serverLoadReceiver;
            this.customReceiver = customReceiver;
            this.cacheService = cacheService;
            cacheExpiryInMinutes = double.Parse(configuration[Configuration.CACHE_SERVER_LOAD_STATISTICS_PER_DAY_EXPIRY_IN_MINUTES]!);
            cacheStatisticsKey = configuration[Configuration.CACHE_STATISTICS_KEY]!;
        }

        #region Endpoints

        [Route("daterange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadEventWrapper>>> GetLoadEventsInDataRange(MessagesInRangeRangeRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"{cacheStatisticsKey}-{request.Key}-{request.From.ToUniversalTime()}-{request.To.ToUniversalTime()}-daterange";
            IEnumerable<LoadEventWrapper>? events = await GetInCacheAsync<IEnumerable<LoadEventWrapper>>(cacheKey);

            if (events == null)
            {
                var options = new InRangeQueryOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
                events = await serverLoadReceiver.ReceiveEventsInRangeAsync(options, cancellationToken);
            }

            await cacheService.SetValueAsync(cacheKey, JsonSerializer.Serialize(events.ToList()), cacheExpiryInMinutes);

            return Ok(events);
        }
        [Route("perday/{key}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetWholeAmountStatisticsInDays(string key, CancellationToken cancellationToken)
        {
            var cacheKey = $"{cacheStatisticsKey}-{key}-perday";

            IEnumerable<LoadAmountStatistics>? statistics = await GetInCacheAsync<IEnumerable<LoadAmountStatistics>>(cacheKey);

            if (statistics == null)
            {
                statistics = await serverLoadReceiver.GetAmountStatisticsInDaysAsync(key, cancellationToken);
            }
            var todayStatistics = await serverLoadReceiver.GetAmountStatisticsLastDayAsync(key, cancellationToken);

            var response = statistics.Where(x => !todayStatistics.Any(y => x.Date == y.Date)).ToList();
            response.AddRange(todayStatistics);

            await cacheService.SetValueAsync(cacheKey, JsonSerializer.Serialize(response), cacheExpiryInMinutes);

            return Ok(response.Select(mapper.Map<LoadAmountStatisticsResponse>));
        }
        [Route("amountrange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetAmountStatisticsInRange(MessageAmountInRangeRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"{cacheStatisticsKey}-{request.Key}-{request.From.ToUniversalTime()}-{request.To.ToUniversalTime()}-{request.TimeSpan}-amountrange";

            IEnumerable<LoadAmountStatistics>? statistics = await GetInCacheAsync<IEnumerable<LoadAmountStatistics>>(cacheKey);

            if (statistics == null)
            {
                var options = new InRangeQueryOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
                statistics = await serverLoadReceiver.GetAmountStatisticsInRangeAsync(options, request.TimeSpan, cancellationToken);
            }

            await cacheService.SetValueAsync(cacheKey, JsonSerializer.Serialize(statistics), cacheExpiryInMinutes);

            return Ok(statistics.Select(mapper.Map<LoadAmountStatisticsResponse>));
        }
        [Route("someevents")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadEventWrapper>>> GetSomeLoadEvents(GetSomeMessagesRequest request, CancellationToken cancellationToken)
        {
            var options = new ReadCertainMessageNumberOptions(request.Key, request.NumberOfMessages, request.StartDate.ToUniversalTime(), request.ReadNew);
            IEnumerable<LoadEventWrapper>? events = await serverLoadReceiver.GetCertainAmountOfEvents(options, cancellationToken);

            return Ok(events);
        }
        [Route("somecustomevents")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<CustomEventWrapper>>> GetSomeCustomEvents(GetSomeMessagesRequest request, CancellationToken cancellationToken)
        {
            var options = new ReadCertainMessageNumberOptions(request.Key, request.NumberOfMessages, request.StartDate.ToUniversalTime(), request.ReadNew);
            IEnumerable<CustomEventWrapper>? events = await customReceiver.GetCertainAmountOfEvents(options, cancellationToken);

            return Ok(events);
        }

        #endregion

        #region Private Helpers

        private async Task<T?> GetInCacheAsync<T>(string key) where T : class
        {
            var json = await cacheService.GetValueAsync(key);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            if (json.TryToDeserialize(out T response))
            {
                return response;
            }
            return null;
        }

        #endregion
    }
}