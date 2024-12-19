using MediatR;

namespace ServerMonitorApi.Command.DeleteStatisticsByKey
{
    public record DeleteStatisticsByKeyCommand(string Key) : IRequest<Unit>;
}
