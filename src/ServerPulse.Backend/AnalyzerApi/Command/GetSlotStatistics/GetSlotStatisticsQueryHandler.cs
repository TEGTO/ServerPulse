using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Wrappers;
using AnalyzerApi.Services.Interfaces;
using MediatR;

namespace AnalyzerApi.Command.GetSlotStatistics
{
    public class GetSlotStatisticsQueryHandler : IRequestHandler<GetSlotStatisticsQuery, SlotStatisticsResponse>
    {
        private readonly IStatisticsCollector<ServerStatistics> serverStatisticsCollector;
        private readonly IStatisticsCollector<ServerLoadStatistics> loadStatisticsCollector;
        private readonly IStatisticsCollector<ServerCustomStatistics> customStatisticsCollector;
        private readonly IEventReceiver<LoadEventWrapper> loadEventReceiver;
        private readonly IEventReceiver<CustomEventWrapper> customEventReceiver;
        private readonly int maxLastEventAmount;

        public GetSlotStatisticsQueryHandler(
            IStatisticsCollector<ServerStatistics> serverStatisticsCollector,
            IStatisticsCollector<ServerLoadStatistics> loadStatisticsCollector,
            IStatisticsCollector<ServerCustomStatistics> customStatisticsCollector,
            IEventReceiver<LoadEventWrapper> loadEventReceiver,
            IEventReceiver<CustomEventWrapper> customEventReceiver,
            IConfiguration configuration)
        {
            this.serverStatisticsCollector = serverStatisticsCollector;
            this.loadStatisticsCollector = loadStatisticsCollector;
            this.customStatisticsCollector = customStatisticsCollector;
            this.loadEventReceiver = loadEventReceiver;
            this.customEventReceiver = customEventReceiver;
            maxLastEventAmount = int.Parse(configuration[Configuration.MAX_EVENT_AMOUNT_TO_GET_IN_SLOT_DATA]!);
        }

        public async Task<SlotStatisticsResponse> Handle(GetSlotStatisticsQuery command, CancellationToken cancellationToken)
        {
            return await GetSlotDataAsync(command.Key, cancellationToken);
        }

        private async Task<SlotStatisticsResponse> GetSlotDataAsync(string key, CancellationToken cancellationToken)
        {
            var options = new ReadCertainMessageNumberOptions(key, maxLastEventAmount, DateTime.UtcNow, false);

            var generalStatsTask = serverStatisticsCollector.ReceiveLastStatisticsAsync(key, cancellationToken);
            var loadStatsTask = loadStatisticsCollector.ReceiveLastStatisticsAsync(key, cancellationToken);
            var customStatsTask = customStatisticsCollector.ReceiveLastStatisticsAsync(key, cancellationToken);
            var loadEventsTask = loadEventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken);
            var customEventsTask = customEventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken);

            await Task.WhenAll(generalStatsTask, loadStatsTask, customStatsTask, loadEventsTask, customEventsTask);

            return new SlotStatisticsResponse
            {
                CollectedDateUTC = DateTime.UtcNow,
                GeneralStatistics = await generalStatsTask,
                LoadStatistics = await loadStatsTask,
                CustomEventStatistics = await customStatsTask,
                LastLoadEvents = await loadEventsTask,
                LastCustomEvents = await customEventsTask
            };
        }
    }
}
