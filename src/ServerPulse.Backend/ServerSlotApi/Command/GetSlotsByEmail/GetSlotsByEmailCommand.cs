using MediatR;
using ServerSlotApi.Dtos;

namespace ServerSlotApi.Command.GetSlotsByEmail
{
    public record GetSlotsByEmailCommand(string? Email, string ContainsString) : IRequest<IEnumerable<ServerSlotResponse>>;
}
