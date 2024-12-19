using AnalyzerApi.Infrastructure.Dtos.Responses.Events;
using AnalyzerApi.Infrastructure.Requests;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetSomeLoadEvents
{
    public record GetSomeLoadEventsQuery(GetSomeMessagesRequest Request) : IRequest<IEnumerable<LoadEventResponse>>;
}
