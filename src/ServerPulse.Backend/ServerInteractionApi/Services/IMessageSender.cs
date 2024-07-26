using ServerPulse.EventCommunication.Events;

namespace ServerInteractionApi.Services
{
    public interface IMessageSender
    {
        public Task SendAliveEventAsync(AliveEvent aliveEvent, CancellationToken cancellationToken);
        public Task SendConfigurationEventAsync(ConfigurationEvent configurationEvent, CancellationToken cancellationToken);
    }
}