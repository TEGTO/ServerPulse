using AnalyzerApi.Infrastructure.Requests;
using AnalyzerApi.Infrastructure.Wrappers;
using MediatR;

namespace AnalyzerApi.Command.GetLoadEventsInDataRange
{
    public record GetLoadEventsInDataRangeQuery(MessagesInRangeRangeRequest Request) : IRequest<IEnumerable<LoadEventWrapper>>;
}
