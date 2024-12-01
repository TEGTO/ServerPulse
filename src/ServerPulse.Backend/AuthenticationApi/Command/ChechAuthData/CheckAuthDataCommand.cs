using MediatR;
using Shared.Dtos.Auth;

namespace AuthenticationApi.Command.ChechAuthData
{
    public record CheckAuthDataCommand(CheckAuthDataRequest Request) : IRequest<CheckAuthDataResponse>;
}
