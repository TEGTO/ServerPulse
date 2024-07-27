using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApi.Services
{
    public interface IMessageSender
    {
        public Task SendPulseEventAsync(PulseEvent pulseEvent, CancellationToken cancellationToken);
        public Task SendConfigurationEventAsync(ConfigurationEvent configurationEvent, CancellationToken cancellationToken);
        public Task SendLoadEventsAsync(LoadEvent[] loadEvents, CancellationToken cancellationToken);
    }
}