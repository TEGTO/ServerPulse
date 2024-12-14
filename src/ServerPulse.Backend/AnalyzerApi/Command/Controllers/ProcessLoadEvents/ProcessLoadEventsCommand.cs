using EventCommunication;
using MediatR;

namespace AnalyzerApi.Command.Controllers.ProcessLoadEvents
{
    public record ProcessLoadEventsCommand(LoadEvent[] Events) : IRequest<Unit>;
}
