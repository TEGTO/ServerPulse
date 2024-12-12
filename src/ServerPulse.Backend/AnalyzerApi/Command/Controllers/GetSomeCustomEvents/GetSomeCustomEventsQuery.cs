using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Requests;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetSomeCustomEvents
{
    public record GetSomeCustomEventsQuery(GetSomeMessagesRequest Request) : IRequest<IEnumerable<CustomEventResponse>>;
}
