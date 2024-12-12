using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AnalyzerApi.Services.Receivers.Statistics;
using MediatR;

namespace AnalyzerApi.Command.Builders.LifecycleStatistics
{
    public class BuildLifecycleStatisticsCommandHandler : IRequestHandler<BuildLifecycleStatisticsCommand, ServerLifecycleStatistics>
    {
        private readonly IEventReceiver<PulseEventWrapper> pulseReceiver;
        private readonly IEventReceiver<ConfigurationEventWrapper> confReceiver;
        private readonly IStatisticsReceiver<ServerLifecycleStatistics> statisticsReceiver;

        public BuildLifecycleStatisticsCommandHandler(IEventReceiver<PulseEventWrapper> pulseReceiver, IEventReceiver<ConfigurationEventWrapper> confReceiver, IStatisticsReceiver<ServerLifecycleStatistics> statisticsReceiver)
        {
            this.pulseReceiver = pulseReceiver;
            this.confReceiver = confReceiver;
            this.statisticsReceiver = statisticsReceiver;
        }

        public async Task<ServerLifecycleStatistics> Handle(BuildLifecycleStatisticsCommand command, CancellationToken cancellationToken)
        {
            var configurationTask = confReceiver.GetLastEventByKeyAsync(command.Key, cancellationToken);
            var pulseTask = pulseReceiver.GetLastEventByKeyAsync(command.Key, cancellationToken);
            var statisticsTask = statisticsReceiver.GetLastStatisticsAsync(command.Key, cancellationToken);

            await Task.WhenAll(configurationTask, pulseTask, statisticsTask);

            var lastConfiguration = await configurationTask;
            var lastPulse = await pulseTask;
            var lastStatistics = await statisticsTask;

            bool isAlive = CalculateIsServerAlive(lastPulse, lastConfiguration);

            var uptime = isAlive ? CalculateServerUptime(lastStatistics) : null;
            var lastUptime = CalculateLastUptime(isAlive, lastStatistics, uptime);

            var statistics = new ServerLifecycleStatistics
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
        private static TimeSpan? CalculateServerUptime(ServerLifecycleStatistics? lastStatistics)
        {
            if (lastStatistics != null && lastStatistics.IsAlive)
            {
                if (lastStatistics.LastPulseDateTimeUTC != null)
                {
                    return lastStatistics.ServerUptime + (DateTime.UtcNow - lastStatistics.LastPulseDateTimeUTC.Value);
                }
                return lastStatistics.ServerUptime;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }
        private static TimeSpan? CalculateLastUptime(bool isAlive, ServerLifecycleStatistics? lastStatistics, TimeSpan? currentUptime)
        {
            if (isAlive)
            {
                return currentUptime;
            }
            else if (lastStatistics != null)
            {
                if (lastStatistics.IsAlive)
                {
                    return CalculateServerUptime(lastStatistics);
                }
                else
                {
                    return lastStatistics.LastServerUptime;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
