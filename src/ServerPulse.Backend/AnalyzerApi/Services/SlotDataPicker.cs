using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;

namespace AnalyzerApi.Services
{
    public class SlotDataPicker : ISlotDataPicker
    {
        #region Fields

        private readonly IStatisticsCollector<ServerStatistics> serverStatisticsCollector;
        private readonly IStatisticsCollector<ServerLoadStatistics> loadStatisticsCollector;
        private readonly IStatisticsCollector<ServerCustomStatistics> customStatisticsCollector;
        private readonly IEventReceiver<LoadEventWrapper> loadEventReceiver;
        private readonly IEventReceiver<CustomEventWrapper> customEventReceiver;
        private readonly int maxLastEventAmount;

        #endregion

        public SlotDataPicker(
            IStatisticsCollector<ServerStatistics> serverStatisticsReceiver,
            IStatisticsCollector<ServerLoadStatistics> loadStatisticsReceiver,
            IStatisticsCollector<ServerCustomStatistics> customStatisticsReceiver,
            IEventReceiver<LoadEventWrapper> loadEventReceiver,
            IEventReceiver<CustomEventWrapper> customEventReceiver,
            IConfiguration configuration)
        {
            this.serverStatisticsCollector = serverStatisticsReceiver;
            this.loadStatisticsCollector = loadStatisticsReceiver;
            this.customStatisticsCollector = customStatisticsReceiver;
            this.loadEventReceiver = loadEventReceiver;
            this.customEventReceiver = customEventReceiver;
            maxLastEventAmount = int.Parse(configuration[Configuration.MAX_EVENT_AMOUNT_TO_GET_IN_SLOT_DATA]!);
        }

        public async Task<SlotData> GetSlotDataAsync(string key, CancellationToken cancellationToken)
        {
            var options = new ReadCertainMessageNumberOptions(key, maxLastEventAmount, DateTime.UtcNow, false);

            var tasks = new Task[]
            {
                serverStatisticsCollector.ReceiveLastStatisticsAsync(key, cancellationToken),
                loadStatisticsCollector.ReceiveLastStatisticsAsync(key, cancellationToken),
                customStatisticsCollector.ReceiveLastStatisticsAsync(key, cancellationToken),
                loadEventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken),
                customEventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken)
            };

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return new SlotData
            {
                GeneralStatistics = await (Task<ServerStatistics?>)tasks[0],
                LoadStatistics = await (Task<ServerLoadStatistics?>)tasks[1],
                CustomEventStatistics = await (Task<ServerCustomStatistics?>)tasks[2],
                LastLoadEvents = await (Task<IEnumerable<LoadEventWrapper>>)tasks[3],
                LastCustomEvents = await (Task<IEnumerable<CustomEventWrapper>>)tasks[4]
            };
        }
    }
}