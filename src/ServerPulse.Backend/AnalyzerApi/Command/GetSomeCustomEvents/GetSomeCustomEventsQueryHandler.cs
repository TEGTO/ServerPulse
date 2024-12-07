using AnalyzerApi.Infrastructure.Wrappers;
using AnalyzerApi.Services.Interfaces;
using MediatR;

namespace AnalyzerApi.Command.GetSomeCustomEvents
{
    public class GetSomeCustomEventsQueryHandler : IRequestHandler<GetSomeCustomEventsQuery, IEnumerable<CustomEventWrapper>>
    {
        private readonly IEventReceiver<CustomEventWrapper> customEventReceiver;

        public GetSomeCustomEventsQueryHandler(IEventReceiver<CustomEventWrapper> customEventReceiver)
        {
            this.customEventReceiver = customEventReceiver;
        }

        public async Task<IEnumerable<CustomEventWrapper>> Handle(GetSomeCustomEventsQuery command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var options = new ReadCertainMessageNumberOptions(request.Key, request.NumberOfMessages, request.StartDate.ToUniversalTime(), request.ReadNew);
            return await customEventReceiver.GetCertainAmountOfEventsAsync(options, cancellationToken);
        }
    }
}
