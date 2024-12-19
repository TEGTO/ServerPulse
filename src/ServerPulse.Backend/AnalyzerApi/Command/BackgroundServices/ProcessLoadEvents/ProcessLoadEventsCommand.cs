using EventCommunication;
using MediatR;

namespace AnalyzerApi.Command.BackgroundServices.ProcessLoadEvents
{
    public record ProcessLoadEventsCommand(LoadEvent[] Events) : IRequest<Unit>;
}
