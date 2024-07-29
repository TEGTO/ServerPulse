using AnalyzerApi.Domain.Models;
using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Services
{
    public class ServerAnalyzer : IServerAnalyzer
    {
        private readonly IMessageReceiver messageReceiver;
        private readonly int pulseEventInterval;

        public ServerAnalyzer(IMessageReceiver messageReceiver, IConfiguration configuration)
        {
            this.messageReceiver = messageReceiver;
            pulseEventInterval = int.Parse(configuration[Configuration.PULSE_EVENT_INTERVAL_IN_MILLISECONDS]!);
        }

        public async Task<ServerStatistics> GetServerStatisticsByKeyAsync(string key, CancellationToken cancellationToken)
        {
            var pulseEventTask = messageReceiver.ReceiveLastPulseEventByKeyAsync(key, cancellationToken);
            var configurationEventTask = messageReceiver.ReceiveLastConfigurationEventByKeyAsync(key, cancellationToken);
            var loadEventTask = messageReceiver.ReceiveLastLoadEventByKeyAsync(key, cancellationToken);
            var loadNumberTask = messageReceiver.ReceiveLoadEventAmountByKeyAsync(key, cancellationToken);

            await Task.WhenAll(pulseEventTask, configurationEventTask, loadEventTask, loadNumberTask);

            PulseEvent? pulseEvent = await pulseEventTask;
            ConfigurationEvent? configurationEvent = await configurationEventTask;
            LoadEvent? loadEvent = await loadEventTask;
            int loadNumber = await loadNumberTask;

            bool isAlive = CalculateIsServerAlive(pulseEvent);
            TimeSpan? uptime = CalculateServerUptime(configurationEvent, isAlive);
            TimeSpan? lastUptime = CalculateServerLastUptime(pulseEvent, configurationEvent);

            ServerStatistics statistics = new ServerStatistics
            {
                IsAlive = isAlive,
                DataExists = configurationEvent != null,
                ServerLastStartDateTime = configurationEvent?.CreationDate,
                ServerUptime = uptime,
                LastServerUptime = lastUptime,
                LastPulseDateTime = pulseEvent?.CreationDate,
                LastLoadDateTime = loadEvent?.CreationDate,
                LoadEventNumber = loadNumber,
            };

            return statistics;
        }
        private bool CalculateIsServerAlive(PulseEvent? pulseEvent)
        {
            bool isAlive = false;
            if (pulseEvent != null)
            {
                bool isEventInInterval = pulseEvent.CreationDate >= DateTime.UtcNow.AddMilliseconds(-1 * pulseEventInterval);
                isAlive = pulseEvent.IsAlive && isEventInInterval;
            }
            return isAlive;
        }
        private TimeSpan? CalculateServerUptime(ConfigurationEvent? configurationEvent, bool isServerAlive)
        {
            if (configurationEvent != null && isServerAlive)
            {
                TimeSpan uptime = DateTime.UtcNow - configurationEvent.CreationDate;
                return uptime;
            }
            return null;
        }
        private TimeSpan? CalculateServerLastUptime(PulseEvent? pulseEvent, ConfigurationEvent? configurationEvent)
        {
            if (pulseEvent != null && configurationEvent != null)
            {
                TimeSpan lastUptime = pulseEvent.CreationDate - configurationEvent.CreationDate;
                return lastUptime;
            }
            return null;
        }
    }
}