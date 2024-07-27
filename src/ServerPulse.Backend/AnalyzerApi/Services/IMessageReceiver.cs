using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Services
{
    public interface IMessageReceiver
    {
        public Task<PulseEvent?> ReceiveLastPulseEventByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<ConfigurationEvent?> ReceiveLastConfigurationEventByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<LoadEvent?> ReceiveLastLoadEventByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<int> ReceiveLoadEventAmountByKeyAsync(string key, CancellationToken cancellationToken);
        public Task<int> ReceiveLoadEventAmountInDateRangeByKeyAsync(string key, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    }
}