using AuthenticationApi.Dtos;
using MediatR;

namespace AuthenticationApi.Command.ChechAuthData
{
    public record CheckAuthDataCommand(CheckAuthDataRequest Request) : IRequest<CheckAuthDataResponse>;
}
