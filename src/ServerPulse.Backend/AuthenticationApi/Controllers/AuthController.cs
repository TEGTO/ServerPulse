using AuthenticationApi.Command;
using AuthenticationApi.Command.ConfirmEmail;
using AuthenticationApi.Command.LoginUser;
using AuthenticationApi.Command.RefreshToken;
using AuthenticationApi.Command.RegisterUser;
using AuthenticationApi.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace AuthenticationApi.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator mediator;

        public AuthController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationRequest request, CancellationToken cancellationToken)
        {
            await mediator.Send(new RegisterUserCommand(request), cancellationToken);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserAuthenticationResponse>> Login(UserAuthenticationRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new LoginUserCommand(request), cancellationToken);
            return Ok(response);
        }

        [FeatureGate(Features.EMAIL_CONFIRMATION)]
        [HttpPost("confirmation")]
        public async Task<ActionResult<UserAuthenticationResponse>> ConfirmEmail(EmailConfirmationRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new ConfirmEmailCommand(request), cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UserUpdateDataRequest request, CancellationToken cancellationToken)
        {
            await mediator.Send(new UpdateUserCommand(request, User), cancellationToken);
            return Ok();
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AccessTokenDataDto>> Refresh(AccessTokenDataDto request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new RefreshTokenCommand(request), cancellationToken);
            return Ok(response);
        }
    }
}