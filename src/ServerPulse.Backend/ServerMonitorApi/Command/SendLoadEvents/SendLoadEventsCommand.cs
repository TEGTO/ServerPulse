using MediatR;
using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApi.Command.SendLoadEvents
{
    public record SendLoadEventsCommand(LoadEvent[] Events) : IRequest<Unit>;
}
