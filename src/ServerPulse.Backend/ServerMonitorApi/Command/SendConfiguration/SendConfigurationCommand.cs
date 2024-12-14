using EventCommunication;
using MediatR;

namespace ServerMonitorApi.Command.SendConfiguration
{
    public record SendConfigurationCommand(ConfigurationEvent Event) : IRequest<Unit>;
}
