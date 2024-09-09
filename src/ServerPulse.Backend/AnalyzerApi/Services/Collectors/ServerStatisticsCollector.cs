using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;

namespace AnalyzerApi.Services.Collectors
{
    public class ServerStatisticsCollector : IStatisticsCollector<ServerStatistics>
    {
        private readonly IEventReceiver<PulseEventWrapper> pulseReceiver;
        private readonly IEventReceiver<ConfigurationEventWrapper> confReceiver;
        private readonly IStatisticsReceiver<ServerStatistics> statisticsReceiver;

        public ServerStatisticsCollector(IEventReceiver<PulseEventWrapper> pulseReceiver, IEventReceiver<ConfigurationEventWrapper> confReceiver, IStatisticsReceiver<ServerStatistics> statisticsReceiver)
        {
            this.pulseReceiver = pulseReceiver;
            this.confReceiver = confReceiver;
            this.statisticsReceiver = statisticsReceiver;
        }

        public async Task<ServerStatistics> ReceiveLastStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            var configurationTask = confReceiver.ReceiveLastEventByKeyAsync(key, cancellationToken);
            var pulseTask = pulseReceiver.ReceiveLastEventByKeyAsync(key, cancellationToken);
            var statisticsTask = statisticsReceiver.ReceiveLastStatisticsAsync(key, cancellationToken);

            await Task.WhenAll(configurationTask, pulseTask, statisticsTask);

            var lastConfiguration = await configurationTask;
            var lastPulse = await pulseTask;
            var lastStatistics = await statisticsTask;

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
            };

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
