using MediatR;
using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Command.ProcessLoadEvent
{
    public record ProcessLoadEventsCommand(LoadEvent[] Events) : IRequest<Unit>;
}
