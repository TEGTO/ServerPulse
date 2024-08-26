using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;

namespace AnalyzerApi.Services.Collectors
{
    public class CustomStatisticsCollector : BaseStatisticsCollector, IStatisticsCollector
    {
        private readonly IEventReceiver<CustomEventWrapper> eventReceiver;

        public CustomStatisticsCollector(IEventReceiver<CustomEventWrapper> eventReceiver, IStatisticsSender statisticsSender, ILogger<CustomStatisticsCollector> logger)
            : base(statisticsSender, logger)
        {
            this.eventReceiver = eventReceiver;
        }

        protected override async Task SendInitialStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            var lastEvent = await eventReceiver.ReceiveLastEventByKeyAsync(key, cancellationToken);

            var statistics = new CustomEventStatistics
            {
                LastEvent = lastEvent,
            };
            await statisticsSender.SendStatisticsAsync(key, statistics, cancellationToken);
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
            await foreach (var ev in eventReceiver.ConsumeEventAsync(key, cancellationToken))
            {
                var statistics = new CustomEventStatistics
                {
                    LastEvent = ev,
                };
                await statisticsSender.SendStatisticsAsync(key, statistics, cancellationToken);
            }
        }
    }
}