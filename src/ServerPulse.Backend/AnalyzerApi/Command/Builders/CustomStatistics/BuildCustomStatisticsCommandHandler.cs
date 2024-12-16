using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using MediatR;

namespace AnalyzerApi.Command.Builders.CustomStatistics
{
    public class BuildCustomStatisticsCommandHandler : IRequestHandler<BuildStatisticsCommand<ServerCustomStatistics>, ServerCustomStatistics>
    {
        private readonly IEventReceiver<CustomEventWrapper> eventReceiver;

        public BuildCustomStatisticsCommandHandler(IEventReceiver<CustomEventWrapper> eventReceiver)
        {
            this.eventReceiver = eventReceiver;
        }

        public async Task<ServerCustomStatistics> Handle(BuildStatisticsCommand<ServerCustomStatistics> command, CancellationToken cancellationToken)
        {
            var lastEvent = await eventReceiver.GetLastEventByKeyAsync(command.Key, cancellationToken);

            return new ServerCustomStatistics
            {
                LastEvent = lastEvent
            };
        }
    }
}
