using MediatR;
using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApi.Command.SendPulse
{
    public record SendPulseCommand(PulseEvent Event) : IRequest<Unit>;
}
