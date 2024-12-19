using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AutoMapper;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetSomeLoadEvents
{
    public class GetSomeLoadEventsQueryHandler : IRequestHandler<GetSomeLoadEventsQuery, IEnumerable<LoadEventResponse>>
    {
        private readonly IEventReceiver<LoadEventWrapper> receiver;
        private readonly IMapper mapper;

        public GetSomeLoadEventsQueryHandler(IEventReceiver<LoadEventWrapper> loadEventReceiver, IMapper mapper)
        {
            this.receiver = loadEventReceiver;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<LoadEventResponse>> Handle(GetSomeLoadEventsQuery command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var options = new GetCertainMessageNumberOptions(request.Key, request.NumberOfMessages, request.StartDate.ToUniversalTime(), request.ReadNew);
            var events = await receiver.GetCertainAmountOfEventsAsync(options, cancellationToken);

            return events.Select(mapper.Map<LoadEventResponse>);
        }
    }
}
