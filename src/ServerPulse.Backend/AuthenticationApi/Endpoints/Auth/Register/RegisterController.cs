using AuthenticationApi.Infrastructure;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Register;
using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

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
