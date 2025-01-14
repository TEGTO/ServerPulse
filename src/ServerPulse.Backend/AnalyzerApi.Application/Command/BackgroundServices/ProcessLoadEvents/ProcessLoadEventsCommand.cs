using EventCommunication;
using MediatR;

namespace AnalyzerApi.Application.Command.BackgroundServices.ProcessLoadEvents
{
    public record ProcessLoadEventsCommand(LoadEvent[] Events) : IRequest<Unit>;
}
