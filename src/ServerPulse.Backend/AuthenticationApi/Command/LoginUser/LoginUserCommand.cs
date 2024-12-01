using AuthenticationApi.Domain.Dtos;
using MediatR;

namespace AuthenticationApi.Command.LoginUser
{
    public record LoginUserCommand(UserAuthenticationRequest Request) : IRequest<UserAuthenticationResponse>;
}
