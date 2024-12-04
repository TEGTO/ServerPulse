using MediatR;
using ServerPulse.EventCommunication;

namespace ServerMonitorApi.Command.SendCustomEvents
{
    public record SendCustomEventsCommand(CustomEventWrapper[] Events) : IRequest<Unit>;
}
