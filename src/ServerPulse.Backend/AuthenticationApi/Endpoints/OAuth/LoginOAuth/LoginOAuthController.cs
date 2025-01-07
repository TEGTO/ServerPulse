﻿using AuthenticationApi.Dtos;
using AuthenticationApi.Dtos.OAuth;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.OAuth.LoginOAuth;
using AuthenticationApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

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

        public LoginOAuthController(Dictionary<OAuthLoginProvider, IOAuthService> oAuthServices, IAuthService authService, IMapper mapper)
        {
            this.oAuthServices = oAuthServices;
            this.authService = authService;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<LoginOAuthResponse>> LoginOAuth(LoginOAuthRequest request, CancellationToken cancellationToken)
        {
            var loginModel = await oAuthServices[request.OAuthLoginProvider].GetProviderModelOnCodeAsync(
                new OAuthAccessCodeParams(request.Code, request.CodeVerifier, request.RedirectUrl), cancellationToken);

            if (loginModel == null)
            {
                return Unauthorized("Login model provider is null!");
            }

            var tokenData = await authService.LoginUserWithProviderAsync(loginModel, cancellationToken);

            var tokenDataDto = mapper.Map<AccessTokenDataDto>(tokenData);

            return Ok(new LoginOAuthResponse
            {
                AccessTokenData = tokenDataDto,
                Email = loginModel.Email,
            });
        }
    }
}