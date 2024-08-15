using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using System.Collections.Concurrent;

namespace AnalyzerApi.Services
{
    public class ServerStatisticsCollector : IStatisticsCollector
    {
        private readonly ConcurrentDictionary<string, ConfigurationEventWrapper> configurations = new();
        private readonly ConcurrentDictionary<string, PulseEventWrapper> lastPulseEvents = new();
        private readonly ConcurrentDictionary<string, ServerStatistics> lastServerStatistics = new();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> statisticsListeners = new();
        private readonly IServerStatusReceiver messageReceiver;
        private readonly IStatisticsSender statisticsSender;
        private readonly PeriodicTimer periodicTimer;
        private readonly ILogger<ServerStatisticsCollector> logger;

        public ServerStatisticsCollector(IServerStatusReceiver messageReceiver, IStatisticsSender statisticsSender, IConfiguration configuration, ILogger<ServerStatisticsCollector> logger)
        {
            this.messageReceiver = messageReceiver;
            this.statisticsSender = statisticsSender;
            this.logger = logger;
            int intervalInMilliseconds = int.Parse(configuration[Configuration.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS]!);
            periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalInMilliseconds));
        }

        public void StartConsumingStatistics(string key)
        {
            _ = Task.Run(async () =>
            {
                await StartConsumingStatisticsAsync(key);
            });
        }
        private async Task StartConsumingStatisticsAsync(string key)
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                await SendInitialStatisticsAsync(key, cancellationToken);

                if (statisticsListeners.TryAdd(key, cancellationTokenSource))
                {
                    var tasks = new[]
                    {
                       SubscribeToPulseEventsAsync(key, cancellationToken),
                       SubscribeToConfigurationEventsAsync(key, cancellationToken),
                       PeriodicallySendStatisticsAsync(key, cancellationToken)
                    };
                    await Task.WhenAll(tasks);
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation($"The operation ConsumeStatisticsAsync with key '{key}' was canceled.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
        public void StopConsumingStatistics(string key)
        {
            if (statisticsListeners.TryRemove(key, out var tokenSource))
            {
                configurations.TryRemove(key, out var configuration);
                lastPulseEvents.TryRemove(key, out var pulse);
                lastServerStatistics.TryRemove(key, out var statistics);

                tokenSource.Cancel();
                tokenSource.Dispose();
            }
        }
        private async Task PeriodicallySendStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            while (await periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                if (lastPulseEvents.TryGetValue(key, out var lastPulse))
                {
                    var statistics = CollectServerStatistics(key, false, cancellationToken);
                    if (!statistics.IsAlive)
                    {
                        lastPulseEvents.TryRemove(key, out lastPulse);
                    }
                    await statisticsSender.SendServerStatisticsAsync(key, statistics, cancellationToken);
                }
                if (configurations.TryGetValue(key, out var lastConfiguration))
                {
                    int waitTime = (int)lastConfiguration.ServerKeepAliveInterval.TotalMilliseconds;
                    await Task.Delay(waitTime);
                }
            }
        }
        private async Task SendInitialStatisticsAsync(string key, CancellationToken cancellationToken)
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

            var statistics = CollectServerStatistics(key, true, cancellationToken);
            statistics.IsInitial = true;

            if (!statistics.IsAlive)
            {
                lastPulseEvents.TryRemove(key, out var lastPulse);
            }

            await statisticsSender.SendServerStatisticsAsync(key, statistics, cancellationToken);
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
        private ServerStatistics CollectServerStatistics(string key, bool isInitial, CancellationToken cancellationToken)
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