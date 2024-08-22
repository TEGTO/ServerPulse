using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApi.Services
{
    public interface IEventProcessing
    {
        public Task SendEventsForProcessingsAsync<T>(T[] events, CancellationToken cancellationToken) where T : BaseEvent;
    }
}