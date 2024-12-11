using EventCommunication.Events;
using MediatR;

namespace ServerMonitorApi.Command.SendConfiguration
{
    public record SendConfigurationCommand(ConfigurationEvent Event) : IRequest<Unit>;
}
