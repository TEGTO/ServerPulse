using AnalyzerApi.Command.Builders.CustomStatistics;
using AnalyzerApi.Command.Builders.LifecycleStatistics;
using AnalyzerApi.Command.Builders.LoadStatistics;
using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AutoMapper;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetSlotStatistics
{
    public class GetSlotStatisticsQueryHandler : IRequestHandler<GetSlotStatisticsQuery, SlotStatisticsResponse>
    {
        private readonly IMediator mediator;
        private readonly IEventReceiver<LoadEventWrapper> loadEventReceiver;
        private readonly IEventReceiver<CustomEventWrapper> customEventReceiver;
        private readonly IMapper mapper;
        private readonly int maxLastEventAmount;

        public GetSlotStatisticsQueryHandler(
            IMediator mediator,
            IEventReceiver<LoadEventWrapper> loadEventReceiver,
            IEventReceiver<CustomEventWrapper> customEventReceiver,
            IMapper mapper,
            IConfiguration configuration)
        {
            this.mediator = mediator;
            this.loadEventReceiver = loadEventReceiver;
            this.customEventReceiver = customEventReceiver;
            this.mapper = mapper;
            maxLastEventAmount = int.Parse(configuration[Configuration.MAX_EVENT_AMOUNT_TO_GET_IN_SLOT_DATA]!);
        }

        public async Task<SlotStatisticsResponse> Handle(GetSlotStatisticsQuery command, CancellationToken cancellationToken)
        {
            var key = command.Key;

            var options = new GetCertainMessageNumberOptions(key, maxLastEventAmount, DateTime.UtcNow, false);

            var generalStatsTask = mediator.Send(new BuildLifecycleStatisticsCommand(key), cancellationToken);
            var loadStatsTask = mediator.Send(new BuildLoadStatisticsCommand(key), cancellationToken);
            var customStatsTask = mediator.Send(new BuildCustomStatisticsCommand(key), cancellationToken);
            var loadEventsTask = loadEventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken);
            var customEventsTask = customEventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken);

            await Task.WhenAll(generalStatsTask, loadStatsTask, customStatsTask, loadEventsTask, customEventsTask);

            return new SlotStatisticsResponse
            {
                CollectedDateUTC = DateTime.UtcNow,
                GeneralStatistics = mapper.Map<ServerLifecycleStatisticsResponse>(await generalStatsTask),
                LoadStatistics = mapper.Map<ServerLoadStatisticsResponse>(await loadStatsTask),
                CustomEventStatistics = mapper.Map<ServerCustomStatisticsResponse>(await customStatsTask),
                LastLoadEvents = (await loadEventsTask).Select(mapper.Map<LoadEventResponse>),
                LastCustomEvents = (await customEventsTask).Select(mapper.Map<CustomEventResponse>)
            };
        }
    }
}
