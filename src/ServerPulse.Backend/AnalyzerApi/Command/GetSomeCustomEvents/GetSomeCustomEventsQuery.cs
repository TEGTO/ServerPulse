using AnalyzerApi.Infrastructure.Requests;
using AnalyzerApi.Infrastructure.Wrappers;
using MediatR;

namespace AnalyzerApi.Command.GetSomeCustomEvents
{
    public record GetSomeCustomEventsQuery(GetSomeMessagesRequest Request) : IRequest<IEnumerable<CustomEventWrapper>>;
}
