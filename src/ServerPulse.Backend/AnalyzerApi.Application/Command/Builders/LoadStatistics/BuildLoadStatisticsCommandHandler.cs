using AnalyzerApi.Application.Services.Receivers.Event;
using AnalyzerApi.Application.Services.Receivers.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Core.Models.Wrappers;
using MediatR;

namespace AnalyzerApi.Application.Command.Builders.LoadStatistics
{
    public sealed class BuildLoadStatisticsCommandHandler : IRequestHandler<BuildStatisticsCommand<ServerLoadStatistics>, ServerLoadStatistics>
    {
        private readonly IEventReceiver<LoadEventWrapper> eventReceiver;
        private readonly IStatisticsReceiver<LoadMethodStatistics> methodStatsReceiver;

        public BuildLoadStatisticsCommandHandler(IEventReceiver<LoadEventWrapper> eventReceiver, IStatisticsReceiver<LoadMethodStatistics> methodStatsReceiver)
        {
            this.eventReceiver = eventReceiver;
            this.methodStatsReceiver = methodStatsReceiver;
        }

        public async Task<ServerLoadStatistics> Handle(BuildStatisticsCommand<ServerLoadStatistics> command, CancellationToken cancellationToken)
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
