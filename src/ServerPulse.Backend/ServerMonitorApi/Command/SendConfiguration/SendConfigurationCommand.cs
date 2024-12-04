using MediatR;
using ServerPulse.EventCommunication.Events;

namespace ServerMonitorApi.Command.SendConfiguration
{
    public record SendConfigurationCommand(ConfigurationEvent Event) : IRequest<Unit>;
}
