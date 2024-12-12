using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AnalyzerApi.Services.Receivers.Statistics;
using MediatR;

namespace AnalyzerApi.Command.Builders.LoadStatistics
{
    public class BuildLoadStatisticsCommandHandler : IRequestHandler<BuildLoadStatisticsCommand, ServerLoadStatistics>
    {
        private readonly IEventReceiver<LoadEventWrapper> eventReceiver;
        private readonly IStatisticsReceiver<LoadMethodStatistics> methodStatsReceiver;

        public BuildLoadStatisticsCommandHandler(IEventReceiver<LoadEventWrapper> eventReceiver, IStatisticsReceiver<LoadMethodStatistics> methodStatsReceiver)
        {
            this.eventReceiver = eventReceiver;
            this.methodStatsReceiver = methodStatsReceiver;
        }

        public async Task<ServerLoadStatistics> Handle(BuildLoadStatisticsCommand command, CancellationToken cancellationToken)
        {
            var amountTask = eventReceiver.GetEventAmountByKeyAsync(command.Key, cancellationToken);
            var loadTask = eventReceiver.GetLastEventByKeyAsync(command.Key, cancellationToken);
            var methodStatisticsTask = methodStatsReceiver.GetLastStatisticsAsync(command.Key, cancellationToken);

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
