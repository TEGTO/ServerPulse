using AuthenticationApi.Dtos;
using MediatR;

namespace AuthenticationApi.Command.RefreshToken
{
    public record RefreshTokenCommand(AccessTokenDataDto Request) : IRequest<AccessTokenDataDto>;
}
