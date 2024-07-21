using MessageBus.Dtos;

namespace Shared.Dtos.ServerEvent
{
    public record AliveEvent(string SlotKey, bool IsAlive) : BaseEvent();
}