using ServerPulse.Client.Events;

namespace ServerPulse.Client.Services
{
    public interface IEventSender
    {
        public Task SendEventAsync(BaseEvent @event, string url, CancellationToken cancellationToken);
    }
}