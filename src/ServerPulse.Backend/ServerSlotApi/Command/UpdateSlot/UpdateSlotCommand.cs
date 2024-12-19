using MediatR;
using ServerSlotApi.Dtos;

namespace ServerSlotApi.Command.UpdateSlot
{
    public record UpdateSlotCommand(string? Email, UpdateServerSlotRequest Request) : IRequest<Unit>;
}
