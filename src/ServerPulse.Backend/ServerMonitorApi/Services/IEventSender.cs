using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApi.Services
{
    public interface IEventSender
    {
        public Task SendEventsAsync<T>(T[] events, CancellationToken cancellationToken) where T : BaseEvent;
    }
}