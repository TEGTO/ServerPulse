using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;

namespace AnalyzerApi.Services
{
    public class LoadStatisticsCollector : BaseStatisticsCollector, IStatisticsCollector
    {
        private readonly IServerLoadReceiver loadReceiver;

        public LoadStatisticsCollector(IServerLoadReceiver loadReceiver, IStatisticsSender statisticsSender, ILogger<LoadStatisticsCollector> logger)
            : base(statisticsSender, logger)
        {
            this.loadReceiver = loadReceiver;
        }

        protected override async Task SendInitialStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            var amountTask = loadReceiver.ReceiveLoadEventAmountByKeyAsync(key, cancellationToken);
            var loadTask = loadReceiver.ReceiveLastLoadEventByKeyAsync(key, cancellationToken);
            var methodStatisticsTask = loadReceiver.ReceiveLastLoadMethodStatisticsByKeyAsync(key, cancellationToken);

            await Task.WhenAll(amountTask, loadTask, methodStatisticsTask);

            int amountOfEvents = await amountTask;
            var lastLoadEvent = await loadTask;
            var methodStatistics = await methodStatisticsTask;

            var statistics = new ServerLoadStatistics
            {
                AmountOfEvents = amountOfEvents,
                LastEvent = lastLoadEvent,
                LoadMethodStatistics = methodStatistics,
                CollectedDateUTC = DateTime.UtcNow,
                IsInitial = true
            };
            await statisticsSender.SendServerLoadStatisticsAsync(key, statistics, cancellationToken);
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
            await foreach (var load in loadReceiver.ConsumeLoadEventAsync(key, cancellationToken))
            {
                var amountTask = loadReceiver.ReceiveLoadEventAmountByKeyAsync(key, cancellationToken);
                var methodStatisticsTask = loadReceiver.ReceiveLastLoadMethodStatisticsByKeyAsync(key, cancellationToken);

                await Task.WhenAll(amountTask, methodStatisticsTask);

                int amountOfEvents = await amountTask;
                var methodStatistics = await methodStatisticsTask;

                var statistics = new ServerLoadStatistics
                {
                    AmountOfEvents = amountOfEvents,
                    LastEvent = load,
                    LoadMethodStatistics = methodStatistics,
                    CollectedDateUTC = DateTime.UtcNow
                };
                await statisticsSender.SendServerLoadStatisticsAsync(key, statistics, cancellationToken);
            }
        }
    }
}