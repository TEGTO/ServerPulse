using MediatR;
using ServerSlotApi.Dtos;

namespace ServerSlotApi.Command.CheckSlotKey
{
    public record CheckSlotKeyCommand(CheckSlotKeyRequest Request) : IRequest<CheckSlotKeyResponse>;
}
