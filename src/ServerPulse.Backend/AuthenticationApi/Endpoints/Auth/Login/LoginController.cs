﻿using AuthenticationApi.Application;
using AuthenticationApi.Application.Services;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.Login;
using AuthenticationApi.Core.Models;
using AutoMapper;
using ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthenticationApi.Endpoints.Auth.Login
{
    [Route("auth")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IFeatureManager featureManager;
        private readonly IMapper mapper;

        public LoginController(IAuthService authService, IFeatureManager featureManager, IMapper mapper)
        {
            this.authService = authService;
            this.featureManager = featureManager;
            this.mapper = mapper;
        }

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Authentication Login.",
            Description = "Logns into system by using email and password."
        )]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
        {
            if (!await CheckEmailConfirmation(request))
            {
                return Unauthorized("Invalid login or password.");
            }

            var loginModel = new LoginUserModel { Login = request.Login, Password = request.Password };
            var tokenData = await authService.LoginUserAsync(loginModel, cancellationToken);

            var tokenDataDto = mapper.Map<LoginAccessTokenData>(tokenData);

            return Ok(new LoginResponse
            {
                AccessTokenData = tokenDataDto,
                Email = request.Login,
            });
        }

        private async Task<bool> CheckEmailConfirmation(LoginRequest request)
        {
            if (await featureManager.IsEnabledAsync(Features.EMAIL_CONFIRMATION)
                && !await authService.CheckEmailConfirmationAsync(request.Login))
            {
                return false;
            }
            return true;
        }
    }
}
