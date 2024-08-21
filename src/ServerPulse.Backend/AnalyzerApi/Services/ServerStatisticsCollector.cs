using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using System.Collections.Concurrent;

namespace AnalyzerApi.Services
{
    public class ServerStatisticsCollector : BaseStatisticsCollector, IStatisticsCollector
    {
        private readonly IServerStatusReceiver messageReceiver;
        private readonly PeriodicTimer periodicTimer;
        private readonly ConcurrentDictionary<string, ConfigurationEventWrapper> configurations = new();
        private readonly ConcurrentDictionary<string, PulseEventWrapper> lastPulseEvents = new();
        private readonly ConcurrentDictionary<string, ServerStatistics> lastServerStatistics = new();

        public ServerStatisticsCollector(IServerStatusReceiver messageReceiver, IStatisticsSender statisticsSender, IConfiguration configuration, ILogger<ServerStatisticsCollector> logger)
            : base(statisticsSender, logger)
        {
            this.messageReceiver = messageReceiver;
            int intervalInMilliseconds = int.Parse(configuration[Configuration.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS]!);
            periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalInMilliseconds));
        }

        protected override async Task SendInitialStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            var configurationTask = messageReceiver.ReceiveLastConfigurationEventByKeyAsync(key, cancellationToken);
            var pulseTask = messageReceiver.ReceiveLastPulseEventByKeyAsync(key, cancellationToken);
            var statisticsTask = messageReceiver.ReceiveLastServerStatisticsByKeyAsync(key, cancellationToken);

            await Task.WhenAll(configurationTask, pulseTask, statisticsTask);

            var configurationEvent = await configurationTask;
            if (configurationEvent != null)
            {
                configurations.TryAdd(key, configurationEvent);
            }

            var pulseEvent = await pulseTask;
            if (pulseEvent != null)
            {
                lastPulseEvents.TryAdd(key, pulseEvent);
            }

            var lastStatistics = await statisticsTask;
            if (lastStatistics != null)
            {
                lastServerStatistics.TryAdd(key, lastStatistics);
            }

            var statistics = CollectServerStatistics(key, true);
            statistics.IsInitial = true;

            if (!statistics.IsAlive)
            {
                lastPulseEvents.TryRemove(key, out _);
            }

            await statisticsSender.SendServerStatisticsAsync(key, statistics, cancellationToken);
        }

        protected override Task[] GetEventSubscriptionTasks(string key, CancellationToken cancellationToken)
        {
            return new[]
            {
                SubscribeToPulseEventsAsync(key, cancellationToken),
                SubscribeToConfigurationEventsAsync(key, cancellationToken),
                PeriodicallySendStatisticsAsync(key, cancellationToken)
            };
        }

        protected override void OnStatisticsListenerRemoved(string key)
        {
            configurations.TryRemove(key, out _);
            lastPulseEvents.TryRemove(key, out _);
            lastServerStatistics.TryRemove(key, out _);
        }

        private async Task PeriodicallySendStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            while (await periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                if (lastPulseEvents.TryGetValue(key, out var lastPulse))
                {
                    var statistics = CollectServerStatistics(key, false);
                    if (!statistics.IsAlive)
                    {
                        lastPulseEvents.TryRemove(key, out _);
                    }
                    await statisticsSender.SendServerStatisticsAsync(key, statistics, cancellationToken);
                }

                if (configurations.TryGetValue(key, out var lastConfiguration))
                {
                    int waitTime = (int)lastConfiguration.ServerKeepAliveInterval.TotalMilliseconds;
                    await Task.Delay(waitTime, cancellationToken);
                }
            }
        }

        private async Task SubscribeToPulseEventsAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var pulse in messageReceiver.ConsumePulseEventAsync(key, cancellationToken))
            {
                lastPulseEvents.AddOrUpdate(key, pulse, (k, p) => pulse);
            }
        }

        private async Task SubscribeToConfigurationEventsAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var configuration in messageReceiver.ConsumeConfigurationEventAsync(key, cancellationToken))
            {
                configurations.AddOrUpdate(key, configuration, (k, c) => configuration);
            }
        }

        private ServerStatistics CollectServerStatistics(string key, bool isInitial)
        {
            lastPulseEvents.TryGetValue(key, out var lastPulse);
            configurations.TryGetValue(key, out var lastConfiguration);
            lastServerStatistics.TryGetValue(key, out var lastStatistics);

            bool isAlive = CalculateIsServerAlive(lastPulse, lastConfiguration);

            TimeSpan? uptime = isAlive ? CalculateServerUptime(lastStatistics) : null;
            TimeSpan? lastUptime = CalculateLastUptime(isAlive, lastStatistics, uptime);

            var statistics = new ServerStatistics
            {
                IsAlive = isAlive,
                DataExists = lastConfiguration != null,
                ServerLastStartDateTimeUTC = lastConfiguration?.CreationDateUTC,
                ServerUptime = uptime,
                LastServerUptime = lastUptime,
                LastPulseDateTimeUTC = lastPulse?.CreationDateUTC,
                CollectedDateUTC = DateTime.UtcNow,
                IsInitial = isInitial,
            };

            lastServerStatistics.AddOrUpdate(key, statistics, (k, s) => statistics);

            return statistics;
        }

        private static bool CalculateIsServerAlive(PulseEventWrapper? pulseEvent, ConfigurationEventWrapper? configurationEvent)
        {
            if (pulseEvent != null && configurationEvent != null)
            {
                bool isEventInInterval = pulseEvent.CreationDateUTC >= DateTime.UtcNow.AddMilliseconds(-1 * configurationEvent.ServerKeepAliveInterval.TotalMilliseconds);
                return pulseEvent.IsAlive && isEventInInterval;
            }
            return false;
        }

        private static TimeSpan? CalculateServerUptime(ServerStatistics? lastStatistics)
        {
            if (lastStatistics != null && lastStatistics.IsAlive)
            {
                return lastStatistics.ServerUptime + (DateTime.UtcNow - lastStatistics.LastPulseDateTimeUTC);
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        private TimeSpan? CalculateLastUptime(bool isAlive, ServerStatistics? lastStatistics, TimeSpan? currentUptime)
        {
            if (isAlive)
            {
                return currentUptime;
            }
            else if (lastStatistics?.IsAlive == true)
            {
                return CalculateServerUptime(lastStatistics);
            }
            else
            {
                return lastStatistics?.LastServerUptime;
            }
        }
    }
}