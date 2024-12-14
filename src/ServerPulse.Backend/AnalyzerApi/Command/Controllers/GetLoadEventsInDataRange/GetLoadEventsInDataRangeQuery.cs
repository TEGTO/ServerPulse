using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Requests;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetLoadEventsInDataRange
{
    public record GetLoadEventsInDataRangeQuery(MessagesInRangeRequest Request) : IRequest<IEnumerable<LoadEventResponse>>;
}
