using AuthenticationApi.Dtos;
using MediatR;
using System.Security.Claims;

namespace AuthenticationApi.Command
{
    public record UpdateUserCommand(UserUpdateDataRequest Request, ClaimsPrincipal UserPrincipal) : IRequest<Unit>;
}
