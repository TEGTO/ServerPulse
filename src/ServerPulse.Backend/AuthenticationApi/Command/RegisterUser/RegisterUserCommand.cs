using AuthenticationApi.Dtos;
using MediatR;

namespace AuthenticationApi.Command.RegisterUser
{
    public record RegisterUserCommand(UserRegistrationRequest Request) : IRequest<UserAuthenticationResponse>;
}
