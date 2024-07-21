using MessageBus.Dtos;

namespace Shared.Dtos.ServerEvent
{
    public record SlotKeyDeletionEvent(string SlotKey) : BaseEvent();
}
