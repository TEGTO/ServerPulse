using AnalyzerApi.Application.Services.Receivers.Event;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Core.Models.Wrappers;
using MediatR;

namespace AnalyzerApi.Application.Command.Builders.CustomStatistics
{
    public sealed class BuildCustomStatisticsCommandHandler : IRequestHandler<BuildStatisticsCommand<ServerCustomStatistics>, ServerCustomStatistics>
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
