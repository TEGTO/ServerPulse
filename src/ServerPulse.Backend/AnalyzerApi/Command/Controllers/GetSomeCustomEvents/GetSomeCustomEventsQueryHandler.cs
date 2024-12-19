using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AutoMapper;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetSomeCustomEvents
{
    public class GetSomeCustomEventsQueryHandler : IRequestHandler<GetSomeCustomEventsQuery, IEnumerable<CustomEventResponse>>
    {
        private readonly IEventReceiver<CustomEventWrapper> receiver;
        private readonly IMapper mapper;

        public GetSomeCustomEventsQueryHandler(IEventReceiver<CustomEventWrapper> customEventReceiver, IMapper mapper)
        {
            this.receiver = customEventReceiver;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<CustomEventResponse>> Handle(GetSomeCustomEventsQuery command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var options = new GetCertainMessageNumberOptions(request.Key, request.NumberOfMessages, request.StartDate.ToUniversalTime(), request.ReadNew);
            var events = await receiver.GetCertainAmountOfEventsAsync(options, cancellationToken);

            return events.Select(mapper.Map<CustomEventResponse>);
        }
    }
}
