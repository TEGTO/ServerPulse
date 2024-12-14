using EventCommunication;
using MediatR;

namespace ServerMonitorApi.Command.SendCustomEvents
{
    public record SendCustomEventsCommand(CustomEventContainer[] Events) : IRequest<Unit>;
}
