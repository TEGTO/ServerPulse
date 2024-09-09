using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;

namespace AnalyzerApi.Services.Consumers
{
    public class ServerStatisticsConsumer : StatisticsConsumer<ServerStatistics, PulseEventWrapper>
    {
        private readonly IEventReceiver<ConfigurationEventWrapper> confReceiver;
        private readonly PeriodicTimer periodicTimer;
        private bool isAlive = true;

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
            int intervalInMilliseconds = int.Parse(configuration[Configuration.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS]!);
            periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalInMilliseconds));
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
            while (await periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                if (isAlive)
                {
                    var statistics = await collector.ReceiveLastStatisticsAsync(key, cancellationToken);
                    isAlive = statistics.IsAlive;
                    await statisticsSender.SendStatisticsAsync(key, statistics, cancellationToken);
                }
                var conf = await confReceiver.ReceiveLastEventByKeyAsync(key, cancellationToken);
                if (conf != null)
                {
                    int waitTime = (int)conf.ServerKeepAliveInterval.TotalMilliseconds;
                    await Task.Delay(waitTime, cancellationToken);
                }
            }
        }
        private async Task SubscribeToPulseEventsAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var pulse in receiver.ConsumeEventAsync(key, cancellationToken))
            {
                isAlive = pulse.IsAlive;
            }
        }

        #endregion
    }
}