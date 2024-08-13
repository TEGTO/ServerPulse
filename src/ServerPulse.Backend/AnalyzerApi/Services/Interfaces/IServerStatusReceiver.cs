using AnalyzerApi.Domain.Dtos.Wrappers;

namespace AnalyzerApi.Services.Interfaces
{
    public interface IServerStatusReceiver
    {
        public IAsyncEnumerable<PulseEventWrapper> ConsumePulseEventAsync(string key, CancellationToken cancellationToken);
        public IAsyncEnumerable<ConfigurationEventWrapper> ConsumeConfigurationEventAsync(string key, CancellationToken cancellationToken);
        public Task<PulseEventWrapper?> ReceiveLastPulseEventByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<ConfigurationEventWrapper?> ReceiveLastConfigurationEventByKeyAsync(string key, CancellationToken cancellationToken);
    }
}