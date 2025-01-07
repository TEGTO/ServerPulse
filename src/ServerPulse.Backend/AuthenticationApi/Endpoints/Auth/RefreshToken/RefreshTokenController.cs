using Authentication.Models;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.RefreshToken;
using AuthenticationApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var tokenData = mapper.Map<AccessTokenData>(request);

            var newToken = await authService.RefreshTokenAsync(tokenData, cancellationToken);

            return Ok(mapper.Map<RefreshTokenResponse>(newToken));
        }
    }
}
