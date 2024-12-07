using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Wrappers;
using AnalyzerApi.Services.Interfaces;
using System.Collections.Concurrent;

namespace AnalyzerApi.Services.Consumers
{
    public class ServerStatisticsConsumer : StatisticsConsumer<ServerStatistics, PulseEventWrapper>
    {
        private readonly IEventReceiver<ConfigurationEventWrapper> confReceiver;
        private readonly ConcurrentDictionary<string, PeriodicTimer> lisetenerTimers = new();
        private readonly ConcurrentDictionary<string, bool> listenersIsAlive = new();
        private readonly ConcurrentDictionary<string, int> listenersDelay = new();
        private readonly int intervalInMilliseconds;

        public ServerStatisticsConsumer(
            IStatisticsCollector<ServerStatistics> collector,
            IEventReceiver<PulseEventWrapper> receiver,
            IEventReceiver<ConfigurationEventWrapper> confReceiver,
            IStatisticsSender statisticsSender,
            IConfiguration configuration,
            ILogger<ServerStatisticsConsumer> logger)
            : base(collector, receiver, statisticsSender, logger)
        {
            this.confReceiver = confReceiver;
            intervalInMilliseconds = int.Parse(configuration[Configuration.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS]!);
        }

        #region StatisticsConsumer Members

        protected override Task[] GetEventSubscriptionTasks(string key, CancellationToken cancellationToken)
        {
            return new[]
            {
                SubscribeToPulseEventsAsync(key, cancellationToken),
                PeriodicallySendStatisticsAsync(key, cancellationToken)
            };
        }

        #endregion

        #region Private Helpers

        private async Task PeriodicallySendStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            listenersIsAlive[key] = true;
            listenersDelay[key] = 1000;
            lisetenerTimers[key] = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalInMilliseconds));
            while (await lisetenerTimers[key].WaitForNextTickAsync(cancellationToken))
            {
                if (listenersIsAlive[key])
                {
                    var statistics = await collector.ReceiveLastStatisticsAsync(key, cancellationToken);
                    listenersIsAlive[key] = statistics.IsAlive;
                    await statisticsSender.SendStatisticsAsync(key, statistics, cancellationToken);
                }
                var conf = await confReceiver.ReceiveLastEventByKeyAsync(key, cancellationToken);
                if (conf != null)
                {
                    listenersDelay[key] = (int)conf.ServerKeepAliveInterval.TotalMilliseconds;
                }
                await Task.Delay(listenersDelay[key], cancellationToken);
            }
        }
        private async Task SubscribeToPulseEventsAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var pulse in receiver.ConsumeEventAsync(key, cancellationToken))
            {
                listenersIsAlive[key] = pulse.IsAlive;
            }
        }

        protected override void OnStatisticsListenerRemoved(string key)
        {
            listenersIsAlive.TryRemove(key, out var res);
            listenersDelay.TryRemove(key, out var res1);
            lisetenerTimers.TryRemove(key, out var res3);
        }

        #endregion
    }
}