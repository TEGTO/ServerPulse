using MessageBus.Dtos;

namespace Shared.Dtos.ServerEvent
{
    public record AliveEvent(string SeverSlotId, bool IsAlive) : BaseEvent(SeverSlotId);
}