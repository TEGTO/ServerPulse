using EventCommunication.Wrappers;
using MediatR;

namespace ServerMonitorApi.Command.SendCustomEvents
{
    public record SendCustomEventsCommand(CustomEventWrapper[] Events) : IRequest<Unit>;
}
