using AnalyzerApi.Domain.Dtos.Requests;
using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ServerMonitorApi.Services;
using System.Text.Json;

namespace AnalyzerApi.Controllers
{
    [Route("analyze")]
    [ApiController]
    public class AnalyzeController : ControllerBase
    {
        #region Fields

        private readonly IMapper mapper;
        private readonly IEventReceiver<LoadEventWrapper> loadEventReceiver;
        private readonly IStatisticsReceiver<LoadAmountStatistics> loadAmountStatisticsReceiver;
        private readonly IEventReceiver<CustomEventWrapper> customEventReceiver;
        private readonly ICacheService cacheService;
        private readonly double cacheExpiryInMinutes;
        private readonly string cacheKey;

        #endregion

        public AnalyzeController(
            IMapper mapper,
            IEventReceiver<LoadEventWrapper> loadEventReceiver,
            IStatisticsReceiver<LoadAmountStatistics> loadAmountStatisticsReceiver,
            IEventReceiver<CustomEventWrapper> eventStatisticsReceiver,
            ICacheService cacheService,
            IConfiguration configuration)
        {
            this.mapper = mapper;
            this.loadEventReceiver = loadEventReceiver;
            this.loadAmountStatisticsReceiver = loadAmountStatisticsReceiver;
            this.customEventReceiver = eventStatisticsReceiver;
            this.cacheService = cacheService;
            cacheExpiryInMinutes = configuration.GetValue<double>(Configuration.CACHE_EXPIRY_IN_MINUTES);
            cacheKey = configuration[Configuration.CACHE_KEY]!;
        }

        #region Endpoints

        [Route("daterange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadEventWrapper>>> GetLoadEventsInDataRange(MessagesInRangeRangeRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"{this.cacheKey}-{request.Key}-{request.From.ToUniversalTime()}-{request.To.ToUniversalTime()}-daterange";
            IEnumerable<LoadEventWrapper>? events = await cacheService.GetInCacheAsync<IEnumerable<LoadEventWrapper>>(cacheKey);

            if (events == null)
            {
                var options = new InRangeQueryOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
                events = await loadEventReceiver.ReceiveEventsInRangeAsync(options, cancellationToken);
            }

            await cacheService.SetValueAsync(cacheKey, JsonSerializer.Serialize(events.ToList()), cacheExpiryInMinutes);

            return Ok(events);
        }
        [Route("perday/{key}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetWholeAmountStatisticsInDays(string key, CancellationToken cancellationToken)
        {
            var cacheKey = $"{this.cacheKey}-{key}-perday";

            IEnumerable<LoadAmountStatistics>? statistics = await cacheService.GetInCacheAsync<IEnumerable<LoadAmountStatistics>>(cacheKey);

            var timeSpan = TimeSpan.FromDays(1);

            if (statistics == null)
            {
                statistics = await loadAmountStatisticsReceiver.GetWholeStatisticsInTimeSpanAsync(key, timeSpan, cancellationToken);
            }

            var options = new InRangeQueryOptions(key, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            var todayStatistics = await loadAmountStatisticsReceiver.GetStatisticsInRangeAsync(options, timeSpan, cancellationToken);

            var response = statistics.Where(x => !todayStatistics.Any(y => x.Date == y.Date)).ToList();
            response.AddRange(todayStatistics);

            await cacheService.SetValueAsync(cacheKey, JsonSerializer.Serialize(response), cacheExpiryInMinutes);

            return Ok(response.Select(mapper.Map<LoadAmountStatisticsResponse>));
        }
        [Route("amountrange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetAmountStatisticsInRange(MessageAmountInRangeRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"{this.cacheKey}-{request.Key}-{request.From.ToUniversalTime()}-{request.To.ToUniversalTime()}-{request.TimeSpan}-amountrange";

            IEnumerable<LoadAmountStatistics>? statistics =
                await cacheService.GetInCacheAsync<IEnumerable<LoadAmountStatistics>>(cacheKey);

            if (statistics == null)
            {
                var options = new InRangeQueryOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
                statistics = await loadAmountStatisticsReceiver.GetStatisticsInRangeAsync(options, request.TimeSpan, cancellationToken);
            }

            await cacheService.SetValueAsync(cacheKey, JsonSerializer.Serialize(statistics), cacheExpiryInMinutes);

            return Ok(statistics.Select(mapper.Map<LoadAmountStatisticsResponse>));
        }
        [Route("someevents")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadEventWrapper>>> GetSomeLoadEvents(GetSomeMessagesRequest request, CancellationToken cancellationToken)
        {
            var options = new ReadCertainMessageNumberOptions(request.Key, request.NumberOfMessages, request.StartDate.ToUniversalTime(), request.ReadNew);
            IEnumerable<LoadEventWrapper>? events = await loadEventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken);

            return Ok(events);
        }
        [Route("somecustomevents")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<CustomEventWrapper>>> GetSomeCustomEvents(GetSomeMessagesRequest request, CancellationToken cancellationToken)
        {
            var options = new ReadCertainMessageNumberOptions(request.Key, request.NumberOfMessages, request.StartDate.ToUniversalTime(), request.ReadNew);
            IEnumerable<CustomEventWrapper>? events = await customEventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken);

            return Ok(events);
        }

        #endregion
    }
}