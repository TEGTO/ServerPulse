using AnalyzerApi.Infrastructure.Wrappers;
using AnalyzerApi.Services.Interfaces;
using MediatR;

namespace AnalyzerApi.Command.GetLoadEventsInDataRange
{
    public class GetLoadEventsInDataRangeQueryHandler : IRequestHandler<GetLoadEventsInDataRangeQuery, IEnumerable<LoadEventWrapper>>
    {
        private readonly IEventReceiver<LoadEventWrapper> loadEventReceiver;

        public GetLoadEventsInDataRangeQueryHandler(IEventReceiver<LoadEventWrapper> loadEventReceiver)
        {
            this.loadEventReceiver = loadEventReceiver;
        }

        public async Task<IEnumerable<LoadEventWrapper>> Handle(GetLoadEventsInDataRangeQuery command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var options = new InRangeQueryOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
            var events = await loadEventReceiver.ReceiveEventsInRangeAsync(options, cancellationToken);

            return events;
        }
    }
}
