using EventCommunication;
using MediatR;

namespace ServerMonitorApi.Command.SendPulse
{
    public record SendPulseCommand(PulseEvent Event) : IRequest<Unit>;
}
