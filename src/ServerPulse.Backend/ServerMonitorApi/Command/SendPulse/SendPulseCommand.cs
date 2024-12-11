using EventCommunication.Events;
using MediatR;

namespace ServerMonitorApi.Command.SendPulse
{
    public record SendPulseCommand(PulseEvent Event) : IRequest<Unit>;
}
