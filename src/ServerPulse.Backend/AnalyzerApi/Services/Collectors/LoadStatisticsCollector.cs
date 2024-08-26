using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;

namespace AnalyzerApi.Services.Collectors
{
    public class LoadStatisticsCollector : BaseStatisticsCollector, IStatisticsCollector
    {
        private readonly IEventReceiver<LoadEventWrapper> eventReceiver;
        private readonly IStatisticsReceiver<LoadMethodStatistics> statisticsReceiver;

        public LoadStatisticsCollector(IEventReceiver<LoadEventWrapper> eventReceiver, IStatisticsReceiver<LoadMethodStatistics> statisticsReceiver, IStatisticsSender statisticsSender, ILogger<LoadStatisticsCollector> logger)
            : base(statisticsSender, logger)
        {
            this.eventReceiver = eventReceiver;
            this.statisticsReceiver = statisticsReceiver;
        }

        protected override async Task SendInitialStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            var amountTask = eventReceiver.ReceiveEventAmountByKeyAsync(key, cancellationToken);
            var loadTask = eventReceiver.ReceiveLastEventByKeyAsync(key, cancellationToken);
            var methodStatisticsTask = statisticsReceiver.ReceiveLastStatisticsByKeyAsync(key, cancellationToken);

            await Task.WhenAll(amountTask, loadTask, methodStatisticsTask);

            int amountOfEvents = await amountTask;
            var lastLoadEvent = await loadTask;
            var methodStatistics = await methodStatisticsTask;

            var statistics = new ServerLoadStatistics
            {
                AmountOfEvents = amountOfEvents,
                LastEvent = lastLoadEvent,
                LoadMethodStatistics = methodStatistics,
                IsInitial = true
            };
            await statisticsSender.SendStatisticsAsync(key, statistics, cancellationToken);
        }

        protected override Task[] GetEventSubscriptionTasks(string key, CancellationToken cancellationToken)
        {
            return new[]
            {
                SubscribeToPulseEventsAsync(key, cancellationToken)
            };
        }

        private async Task SubscribeToPulseEventsAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var load in eventReceiver.ConsumeEventAsync(key, cancellationToken))
            {
                var amountTask = eventReceiver.ReceiveEventAmountByKeyAsync(key, cancellationToken);
                var methodStatisticsTask = statisticsReceiver.ReceiveLastStatisticsByKeyAsync(key, cancellationToken);

                await Task.WhenAll(amountTask, methodStatisticsTask);

                int amountOfEvents = await amountTask;
                var methodStatistics = await methodStatisticsTask;

                var statistics = new ServerLoadStatistics
                {
                    AmountOfEvents = amountOfEvents,
                    LastEvent = load,
                    LoadMethodStatistics = methodStatistics,
                };
                await statisticsSender.SendStatisticsAsync(key, statistics, cancellationToken);
            }
        }
    }
}