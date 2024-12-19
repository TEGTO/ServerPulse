using MediatR;

namespace ServerSlotApi.Command.DeleteSlot
{
    public record DeleteSlotCommand(string? Email, string Id, string Token) : IRequest<Unit>;
}
