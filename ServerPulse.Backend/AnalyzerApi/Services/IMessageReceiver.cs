using EventCommunication.Events;

namespace AnalyzerApi.Services
{
    public interface IMessageReceiver
    {
        public Task<AliveEvent> ReceiveLastAliveEventByKeyAsync(string key);
    }
}