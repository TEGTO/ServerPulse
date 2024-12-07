using AnalyzerApi.Infrastructure.Requests;
using AnalyzerApi.Infrastructure.Wrappers;
using MediatR;

namespace AnalyzerApi.Command.GetSomeLoadEvents
{
    public record GetSomeLoadEventsQuery(GetSomeMessagesRequest Request) : IRequest<IEnumerable<LoadEventWrapper>>;
}
