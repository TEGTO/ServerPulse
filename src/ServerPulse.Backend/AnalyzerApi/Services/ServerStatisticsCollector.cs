using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using ServerPulse.EventCommunication.Events;
using System.Collections.Concurrent;

namespace AnalyzerApi.Services
{
    public class ServerStatisticsCollector : IStatisticsCollector
    {
        private readonly ConcurrentDictionary<string, ConfigurationEvent> Configurations = new();
        private readonly ConcurrentDictionary<string, PulseEvent> LastPulseEvents = new();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> StatisticsListeners = new();
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

                if (StatisticsListeners.TryAdd(key, cancellationTokenSource))
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
            if (StatisticsListeners.TryRemove(key, out var tokenSource))
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
            }
        }
        private async Task PeriodicallySendStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            while (await periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                if (LastPulseEvents.TryGetValue(key, out var lastPulse))
                {
                    var statistics = CollectServerStatistics(key, cancellationToken);
                    if (!statistics.IsAlive)
                    {
                        LastPulseEvents.TryRemove(key, out lastPulse);
                    }
                    await statisticsSender.SendServerStatisticsAsync(key, statistics);
                }
                if (Configurations.TryGetValue(key, out var lastConfiguration))
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

            await Task.WhenAll(configurationTask, pulseTask);

            var configurationEvent = await configurationTask;
            if (configurationEvent != null)
            {
                Configurations.TryAdd(key, configurationEvent);
            }

            var pulseEvent = await pulseTask;
            if (pulseEvent != null)
            {
                LastPulseEvents.TryAdd(key, pulseEvent);
            }

            var statistics = CollectServerStatistics(key, cancellationToken);
            if (!statistics.IsAlive)
            {
                LastPulseEvents.TryRemove(key, out var lastPulse);
            }

            await statisticsSender.SendServerStatisticsAsync(key, statistics);
        }
        private async Task SubscribeToPulseEventsAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var pulse in messageReceiver.ConsumePulseEventAsync(key, cancellationToken))
            {
                LastPulseEvents.AddOrUpdate(key, pulse, (k, p) => pulse);
            }
        }
        private async Task SubscribeToConfigurationEventsAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var configuration in messageReceiver.ConsumeConfigurationEventAsync(key, cancellationToken))
            {
                Configurations.AddOrUpdate(key, configuration, (k, c) => configuration);
            }
        }
        private ServerStatistics CollectServerStatistics(string key, CancellationToken cancellationToken)
        {
            LastPulseEvents.TryGetValue(key, out var lastPulse);
            Configurations.TryGetValue(key, out var lastConfiguration);

            bool isAlive = CalculateIsServerAlive(lastPulse, lastConfiguration);
            TimeSpan? uptime = CalculateServerUptime(lastConfiguration, isAlive);
            TimeSpan? lastUptime = CalculateServerLastUptime(lastPulse, lastConfiguration);

            return new ServerStatistics
            {
                IsAlive = isAlive,
                DataExists = lastConfiguration != null,
                ServerLastStartDateTime = lastConfiguration?.CreationDate,
                ServerUptime = uptime,
                LastServerUptime = lastUptime,
                LastPulseDateTime = lastPulse?.CreationDate,
            };
        }
        private static bool CalculateIsServerAlive(PulseEvent? pulseEvent, ConfigurationEvent? configurationEvent)
        {
            if (pulseEvent != null && configurationEvent != null)
            {
                bool isEventInInterval = pulseEvent.CreationDate >= DateTime.UtcNow.AddMilliseconds(-1 * configurationEvent.ServerKeepAliveInterval.TotalMilliseconds);
                return pulseEvent.IsAlive && isEventInInterval;
            }
            return false;
        }
        private static TimeSpan? CalculateServerUptime(ConfigurationEvent? configurationEvent, bool isServerAlive)
        {
            if (configurationEvent != null && isServerAlive)
            {
                return DateTime.UtcNow - configurationEvent.CreationDate;
            }
            return null;
        }
        private static TimeSpan? CalculateServerLastUptime(PulseEvent? pulseEvent, ConfigurationEvent? configurationEvent)
        {
            if (pulseEvent != null && configurationEvent != null)
            {
                return pulseEvent.CreationDate - configurationEvent.CreationDate;
            }
            return null;
        }
    }
}