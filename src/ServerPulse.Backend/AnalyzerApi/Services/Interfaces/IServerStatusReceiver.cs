using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Services.Interfaces
{
    public interface IServerStatusReceiver
    {
        public IAsyncEnumerable<PulseEvent> ConsumePulseEventAsync(string key, CancellationToken cancellationToken);
        public IAsyncEnumerable<ConfigurationEvent> ConsumeConfigurationEventAsync(string key, CancellationToken cancellationToken);
        public Task<PulseEvent?> ReceiveLastPulseEventByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<ConfigurationEvent?> ReceiveLastConfigurationEventByKeyAsync(string key, CancellationToken cancellationToken);
    }
}