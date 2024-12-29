using AuthenticationApi.Dtos;
using MediatR;

namespace AuthenticationApi.Command.ConfirmEmail
{
    public record ConfirmEmailCommand(EmailConfirmationRequest Request) : IRequest<UserAuthenticationResponse>;
}
