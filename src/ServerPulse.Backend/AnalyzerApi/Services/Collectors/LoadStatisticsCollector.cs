using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Wrappers;
using AnalyzerApi.Services.Interfaces;

namespace AnalyzerApi.Services.Collectors
{
    public class LoadStatisticsCollector : IStatisticsCollector<ServerLoadStatistics>
    {
        private readonly IEventReceiver<LoadEventWrapper> eventReceiver;
        private readonly IStatisticsReceiver<LoadMethodStatistics> methodStatsReceiver;

        public LoadStatisticsCollector(IEventReceiver<LoadEventWrapper> eventReceiver, IStatisticsReceiver<LoadMethodStatistics> methodStatsReceiver)
        {
            this.eventReceiver = eventReceiver;
            this.methodStatsReceiver = methodStatsReceiver;
        }

        public async Task<ServerLoadStatistics> ReceiveLastStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            var amountTask = eventReceiver.ReceiveEventAmountByKeyAsync(key, cancellationToken);
            var loadTask = eventReceiver.ReceiveLastEventByKeyAsync(key, cancellationToken);
            var methodStatisticsTask = methodStatsReceiver.ReceiveLastStatisticsAsync(key, cancellationToken);

            await Task.WhenAll(amountTask, loadTask, methodStatisticsTask);

            int amountOfEvents = await amountTask;
            var lastLoadEvent = await loadTask;
            var methodStatistics = await methodStatisticsTask;

            return new ServerLoadStatistics
            {
                AmountOfEvents = amountOfEvents,
                LastEvent = lastLoadEvent,
                LoadMethodStatistics = methodStatistics ?? new LoadMethodStatistics()
            };
        }
    }
}
