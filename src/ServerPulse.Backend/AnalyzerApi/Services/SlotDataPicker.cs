using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;

namespace AnalyzerApi.Services
{
    public class SlotDataPicker : ISlotDataPicker
    {
        #region Fields

        private readonly IStatisticsReceiver<ServerStatistics> serverStatisticsReceiver;
        private readonly IStatisticsReceiver<ServerLoadStatistics> loadStatisticsReceiver;
        private readonly IStatisticsReceiver<CustomEventStatistics> customStatisticsReceiver;
        private readonly IEventReceiver<LoadEventWrapper> loadEventReceiver;
        private readonly IEventReceiver<CustomEventWrapper> customEventReceiver;
        private readonly int maxLastEventAmount;

        #endregion

        public SlotDataPicker(
            IStatisticsReceiver<ServerStatistics> serverStatisticsReceiver,
            IStatisticsReceiver<ServerLoadStatistics> loadStatisticsReceiver,
            IStatisticsReceiver<CustomEventStatistics> customStatisticsReceiver,
            IEventReceiver<LoadEventWrapper> loadEventReceiver,
            IEventReceiver<CustomEventWrapper> customEventReceiver,
            IConfiguration configuration)
        {
            this.serverStatisticsReceiver = serverStatisticsReceiver;
            this.loadStatisticsReceiver = loadStatisticsReceiver;
            this.customStatisticsReceiver = customStatisticsReceiver;
            this.loadEventReceiver = loadEventReceiver;
            this.customEventReceiver = customEventReceiver;
            maxLastEventAmount = configuration.GetValue<int>(Configuration.MAX_EVENT_AMOUNT_TO_GET_IN_SLOT_DATA);
        }

        public async Task<SlotData> GetSlotDataAsync(string key, CancellationToken cancellationToken)
        {
            var options = new ReadCertainMessageNumberOptions(key, maxLastEventAmount, DateTime.UtcNow, false);

            var tasks = new Task[]
            {
                serverStatisticsReceiver.ReceiveLastStatisticsByKeyAsync(key, cancellationToken),
                loadStatisticsReceiver.ReceiveLastStatisticsByKeyAsync(key, cancellationToken),
                customStatisticsReceiver.ReceiveLastStatisticsByKeyAsync(key, cancellationToken),
                loadEventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken),
                customEventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken)
            };

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return new SlotData
            {
                GeneralStatistics = await (Task<ServerStatistics?>)tasks[0],
                LoadStatistics = await (Task<ServerLoadStatistics?>)tasks[1],
                CustomEventStatistics = await (Task<CustomEventStatistics?>)tasks[2],
                LastLoadEvents = await (Task<IEnumerable<LoadEventWrapper>>)tasks[3],
                LastCustomEvents = await (Task<IEnumerable<CustomEventWrapper>>)tasks[4]
            };
        }
    }
}