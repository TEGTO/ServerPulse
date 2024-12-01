using AuthenticationApi.Command;
using AuthenticationApi.Command.ChechAuthData;
using AuthenticationApi.Command.LoginUser;
using AuthenticationApi.Command.RefreshToken;
using AuthenticationApi.Command.RegisterUser;
using AuthenticationApi.Domain.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.Auth;

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
            var response = await mediator.Send(new RegisterUserCommand(request), cancellationToken);
            return CreatedAtAction(nameof(Register), new { id = response.Email }, response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserAuthenticationResponse>> Login(UserAuthenticationRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new LoginUserCommand(request), cancellationToken);
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
        public async Task<ActionResult<AuthToken>> Refresh(AuthToken request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new RefreshTokenCommand(request), cancellationToken);
            return Ok(response);
        }

        [HttpPost("check")]
        public async Task<ActionResult<CheckAuthDataResponse>> CheckAuthData([FromBody] CheckAuthDataRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new CheckAuthDataCommand(request), cancellationToken);
            return Ok(response);
        }
    }
}