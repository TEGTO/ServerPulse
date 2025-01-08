using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.UserUpdate;
using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationApi.Endpoints.Auth.UserUpdate
{
    [Route("auth")]
    [ApiController]
    public class UserUpdateController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public UserUpdateController(IAuthService authService, IMapper mapper)
        {
            this.authService = authService;
            this.mapper = mapper;
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<ActionResult> UserUpdate(UserUpdateRequest request, CancellationToken cancellationToken)
        {
            var updateModel = mapper.Map<UserUpdateModel>(request);

            var errors = await authService.UpdateUserAsync(User, updateModel, false, cancellationToken);
            if (Utilities.HasErrors(errors, out var errorResponse))
            {
                return Unauthorized(errorResponse);
            }

            return Ok();
        }
    }
}
