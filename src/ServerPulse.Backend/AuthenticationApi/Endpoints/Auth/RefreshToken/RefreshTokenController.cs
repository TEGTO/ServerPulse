using Authentication.Models;
using AuthenticationApi.Application.Services;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.RefreshToken;
using AutoMapper;
using ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthenticationApi.Endpoints.Auth.RefreshToken
{
    [Route("auth")]
    [ApiController]
    public class RefreshTokenController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public RefreshTokenController(IAuthService authService, IMapper mapper)
        {
            this.authService = authService;
            this.mapper = mapper;
        }

        [HttpPost("refresh")]
        [SwaggerOperation(
            Summary = "Authentication Token Refresh.",
            Description = "Refreshes access token by using the refresh token."
        )]
        [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var tokenData = mapper.Map<AccessTokenData>(request);

            var newToken = await authService.RefreshTokenAsync(tokenData, cancellationToken);

            return Ok(mapper.Map<RefreshTokenResponse>(newToken));
        }
    }
}
