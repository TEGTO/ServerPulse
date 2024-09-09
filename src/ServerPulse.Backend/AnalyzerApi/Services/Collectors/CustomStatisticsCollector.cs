using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;

namespace AnalyzerApi.Services.Collectors
{
    public class CustomStatisticsCollector : IStatisticsCollector<ServerCustomStatistics>
    {
        private readonly IEventReceiver<CustomEventWrapper> eventReceiver;

        public CustomStatisticsCollector(IEventReceiver<CustomEventWrapper> eventReceiver)
        {
            this.eventReceiver = eventReceiver;
        }

        public async Task<ServerCustomStatistics> ReceiveLastStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            var lastEvent = await eventReceiver.ReceiveLastEventByKeyAsync(key, cancellationToken);

            return new ServerCustomStatistics
            {
                LastEvent = lastEvent
            };
        }
    }
}
