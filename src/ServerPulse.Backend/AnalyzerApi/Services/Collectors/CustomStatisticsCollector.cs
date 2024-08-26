using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;

namespace AnalyzerApi.Services.Collectors
{
    public class CustomStatisticsCollector : BaseStatisticsCollector, IStatisticsCollector
    {
        private readonly ICustomReceiver receiver;

        public CustomStatisticsCollector(ICustomReceiver receiver, IStatisticsSender statisticsSender, ILogger<CustomStatisticsCollector> logger)
            : base(statisticsSender, logger)
        {
            this.receiver = receiver;
        }

        protected override async Task SendInitialStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            var lastEvent = await receiver.ReceiveLastCustomEventByKeyAsync(key, cancellationToken);

            var statistics = new CustomEventStatistics
            {
                LastEvent = lastEvent,
            };
            await statisticsSender.SendServerCustomStatisticsAsync(key, statistics, cancellationToken);
        }

        protected override Task[] GetEventSubscriptionTasks(string key, CancellationToken cancellationToken)
        {
            return new[]
            {
                SubscribeToCustomEventsAsync(key, cancellationToken)
            };
        }

        private async Task SubscribeToCustomEventsAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var ev in receiver.ConsumeCustomEventAsync(key, cancellationToken))
            {
                var statistics = new CustomEventStatistics
                {
                    LastEvent = ev,
                };
                await statisticsSender.SendServerCustomStatisticsAsync(key, statistics, cancellationToken);
            }
        }
    }
}