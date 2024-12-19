using MediatR;
using ServerSlotApi.Dtos;

namespace ServerSlotApi.Command.CreateSlot
{
    public record CreateSlotCommand(string? Email, CreateServerSlotRequest Request) : IRequest<ServerSlotResponse>;
}
