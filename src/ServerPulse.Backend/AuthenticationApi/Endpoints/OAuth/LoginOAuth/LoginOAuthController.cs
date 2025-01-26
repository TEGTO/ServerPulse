using AuthenticationApi.Application;
using AuthenticationApi.Application.Services;
using AuthenticationApi.Core.Dtos.Endpoints.OAuth.LoginOAuth;
using AuthenticationApi.Core.Enums;
using AutoMapper;
using ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthenticationApi.Endpoints.OAuth.LoginOAuth
{
    [FeatureGate(Features.OAUTH)]
    [Route("oauth")]
    [ApiController]
    public class LoginOAuthController : ControllerBase
    {
        private readonly Dictionary<OAuthLoginProvider, IOAuthService> oAuthServices;
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public LoginOAuthController(Dictionary<OAuthLoginProvider, IOAuthService> oAuthServices,
            IAuthService authService,
            IMapper mapper)
        {
            this.oAuthServices = oAuthServices;
            this.authService = authService;
            this.mapper = mapper;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Login by using OAuth.",
            Description = "Logins in the system using OAuth code."
        )]
        [ProducesResponseType(typeof(LoginOAuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginOAuthResponse>> LoginOAuth(LoginOAuthRequest request, CancellationToken cancellationToken)
        {
            var loginModel = await oAuthServices[request.OAuthLoginProvider].GetProviderModelOnCodeAsync(
                request.QueryParams, request.RedirectUrl, cancellationToken);

            if (loginModel == null)
            {
                return Unauthorized("Login model provider is null!");
            }

            var tokenData = await authService.LoginUserWithProviderAsync(loginModel, cancellationToken);

            var tokenDataDto = mapper.Map<LoginOAuthAccessTokenData>(tokenData);

            return Ok(new LoginOAuthResponse
            {
                AccessTokenData = tokenDataDto,
                Email = loginModel.Email,
            });
        }
    }
}
