using AnalyzerApi.Infrastructure.Wrappers;
using AnalyzerApi.Services.Interfaces;
using MediatR;

namespace AnalyzerApi.Command.GetSomeLoadEvents
{
    public class GetSomeLoadEventsQueryHandler : IRequestHandler<GetSomeLoadEventsQuery, IEnumerable<LoadEventWrapper>>
    {
        private readonly IEventReceiver<LoadEventWrapper> loadEventReceiver;

        public GetSomeLoadEventsQueryHandler(IEventReceiver<LoadEventWrapper> loadEventReceiver)
        {
            this.loadEventReceiver = loadEventReceiver;
        }

        public async Task<IEnumerable<LoadEventWrapper>> Handle(GetSomeLoadEventsQuery command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var options = new ReadCertainMessageNumberOptions(request.Key, request.NumberOfMessages, request.StartDate.ToUniversalTime(), request.ReadNew);
            return await loadEventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken);
        }
    }
}
