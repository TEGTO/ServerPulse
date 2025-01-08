using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.ConfirmEmail;
using AuthenticationApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace AuthenticationApi.Endpoints.Auth.ConfirmEmail
{
    [Route("auth")]
    [ApiController]
    public class ConfirmEmailController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public ConfirmEmailController(IAuthService authService, IMapper mapper)
        {
            this.authService = authService;
            this.mapper = mapper;
        }

        [FeatureGate(Features.EMAIL_CONFIRMATION)]
        [HttpPost("confirmation")]
        public async Task<ActionResult<ConfirmEmailResponse>> ConfirmEmail(ConfirmEmailRequest request, CancellationToken cancellationToken)
        {
            var result = await authService.ConfirmEmailAsync(request.Email, request.Token);

            if (Utilities.HasErrors(result.Errors, out var errorResponse))
            {
                return Conflict(errorResponse);
            }

            var tokenData = await authService.LoginUserAfterConfirmationAsync(request.Email, cancellationToken);

            var tokenDataDto = mapper.Map<ConfirmEmailAccessTokenData>(tokenData);

            return Ok(new ConfirmEmailResponse
            {
                AccessTokenData = tokenDataDto,
                Email = request.Email,
            });
        }
    }
}
