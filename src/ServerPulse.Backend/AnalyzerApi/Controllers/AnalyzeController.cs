using AnalyzerApi.Domain.Dtos.Requests;
using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

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

        #endregion

        public AnalyzeController(
            IMapper mapper,
            IEventReceiver<LoadEventWrapper> loadEventReceiver,
            IStatisticsReceiver<LoadAmountStatistics> loadAmountStatisticsReceiver,
            IEventReceiver<CustomEventWrapper> eventStatisticsReceiver,
            IConfiguration configuration)
        {
            this.mapper = mapper;
            this.loadEventReceiver = loadEventReceiver;
            this.loadAmountStatisticsReceiver = loadAmountStatisticsReceiver;
            this.customEventReceiver = eventStatisticsReceiver;
        }

        #region Endpoints

        [OutputCache(PolicyName = "GetLoadEventsInDataRangePolicy")]
        [Route("daterange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadEventWrapper>>> GetLoadEventsInDataRange(MessagesInRangeRangeRequest request, CancellationToken cancellationToken)
        {
            var options = new InRangeQueryOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
            var events = await loadEventReceiver.ReceiveEventsInRangeAsync(options, cancellationToken);

            return Ok(events);
        }

        [OutputCache(PolicyName = "GetWholeAmountStatisticsInDaysPolicy")]
        [Route("perday/{key}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetWholeAmountStatisticsInDays(string key, CancellationToken cancellationToken)
        {
            var timeSpan = TimeSpan.FromDays(1);

            var statistics = await loadAmountStatisticsReceiver.GetWholeStatisticsInTimeSpanAsync(key, timeSpan, cancellationToken);

            var options = new InRangeQueryOptions(key, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            var todayStatistics = await loadAmountStatisticsReceiver.GetStatisticsInRangeAsync(options, timeSpan, cancellationToken);

            var response = statistics.Where(x => !todayStatistics.Any(y => x.DateFrom > y.DateFrom || x.DateTo > y.DateFrom)).ToList();
            response.AddRange(todayStatistics);

            return Ok(response.Select(mapper.Map<LoadAmountStatisticsResponse>));
        }


        [OutputCache(PolicyName = "GetAmountStatisticsInRangePolicy")]
        [Route("amountrange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetAmountStatisticsInRange(MessageAmountInRangeRequest request, CancellationToken cancellationToken)
        {
            var options = new InRangeQueryOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
            var statistics = await loadAmountStatisticsReceiver.GetStatisticsInRangeAsync(options, request.TimeSpan, cancellationToken);

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