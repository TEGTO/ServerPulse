using AuthenticationApi.Dtos;
using AuthenticationApi.Dtos.OAuth;
using MediatR;

namespace AuthenticationApi.Command.LoginOAuthCommand
{
    public record LoginOAuthCommand(UserOAuthenticationRequest Request) : IRequest<UserAuthenticationResponse>;
}
