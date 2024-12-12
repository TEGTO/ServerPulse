using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AutoMapper;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetLoadEventsInDataRange
{
    public class GetLoadEventsInDataRangeQueryHandler : IRequestHandler<GetLoadEventsInDataRangeQuery, IEnumerable<LoadEventResponse>>
    {
        private readonly IEventReceiver<LoadEventWrapper> receiver;
        private readonly IMapper mapper;

        public GetLoadEventsInDataRangeQueryHandler(IEventReceiver<LoadEventWrapper> loadEventReceiver, IMapper mapper)
        {
            this.receiver = loadEventReceiver;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<LoadEventResponse>> Handle(GetLoadEventsInDataRangeQuery command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var options = new InRangeQuery(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
            var events = await receiver.GetEventsInRangeAsync(options, cancellationToken);

            return events.Select(mapper.Map<LoadEventResponse>);
        }
    }
}
