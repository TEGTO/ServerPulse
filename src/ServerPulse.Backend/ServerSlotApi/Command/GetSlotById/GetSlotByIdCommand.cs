using MediatR;
using ServerSlotApi.Dtos;

namespace ServerSlotApi.Command.GetSlotById
{
    public record GetSlotByIdCommand(string? Email, string Id) : IRequest<ServerSlotResponse>;
}
