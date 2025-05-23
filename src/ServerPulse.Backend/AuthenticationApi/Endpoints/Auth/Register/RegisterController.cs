﻿using AuthenticationApi.Application;
using AuthenticationApi.Application.Services;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.Register;
using AuthenticationApi.Core.Entities;
using AuthenticationApi.Core.Models;
using AutoMapper;
using ExceptionHandling;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthenticationApi.Endpoints.Auth.Register
{
    [Route("auth")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IFeatureManager featureManager;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IEmailJobService emailJobService;
        private readonly IMapper mapper;

        public RegisterController(
            IAuthService authService,
            IFeatureManager featureManager,
            IBackgroundJobClient backgroundJobClient,
            IEmailJobService emailJobService,
            IMapper mapper)
        {
            this.authService = authService;
            this.featureManager = featureManager;
            this.backgroundJobClient = backgroundJobClient;
            this.mapper = mapper;
            this.emailJobService = emailJobService;
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Authentication Registration.",
            Description = "Register user by using email and password."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
        {
            var user = mapper.Map<User>(request);

            var registerModel = new RegisterUserModel { User = user, Password = request.Password };

            var errors = (await authService.RegisterUserAsync(registerModel, cancellationToken)).Errors;
            if (Utilities.HasErrors(errors, out var errorResponse))
            {
                return Conflict(errorResponse);
            }

            if (await featureManager.IsEnabledAsync(Features.EMAIL_CONFIRMATION))
            {
                backgroundJobClient.Enqueue(() => emailJobService.SendEmailConfirmationMessageAsync(request));
            }

            return Ok();
        }
    }
}
