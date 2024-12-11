using EventCommunication.Events;
using MediatR;

namespace AnalyzerApi.Command.ProcessLoadEvent
{
    public record ProcessLoadEventsCommand(LoadEvent[] Events) : IRequest<Unit>;
}
