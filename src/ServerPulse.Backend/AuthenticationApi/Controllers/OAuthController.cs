using AuthenticationApi.Command.GetOAuthUrlQuery;
using AuthenticationApi.Command.LoginOAuthCommand;
using AuthenticationApi.Dtos;
using AuthenticationApi.Dtos.OAuth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationApi.Controllers
{
    [Route("oauth")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly IMediator mediator;

        public OAuthController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<GetOAuthUrlResponse>> GetOAuthUrl([FromQuery] GetOAuthUrlQueryParams queryParams, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetOAuthUrlQuery(queryParams), cancellationToken);
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<UserAuthenticationResponse>> LoginOAuth(UserOAuthenticationRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new LoginOAuthCommand(request), cancellationToken);
            return Ok(response);
        }
    }
}
