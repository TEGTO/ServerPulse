using MediatR;

namespace ServerSlotApi.Command.DeleteSlot
{
    public record DeleteSlotCommand(string? Email, string Id) : IRequest<Unit>;
}
