using Authentication.Models;
using AuthenticationApi.Domain.Dtos;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Domain.Models;
using AuthenticationApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using System.Net;

namespace AuthenticationApi.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IAuthService authService;
        private readonly IConfiguration configuration;

        public AuthController(IMapper mapper, IAuthService authService, IConfiguration configuration)
        {
            this.mapper = mapper;
            this.authService = authService;
            this.configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest registrationRequest)
        {
            if (registrationRequest == null)
            {
                return BadRequest("Invalid client request");
            }
            var user = mapper.Map<User>(registrationRequest);
            var result = await authService.RegisterUserAsync(user, registrationRequest.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Where(e => !e.Description.Contains("Username"))
                    .Select(e => e.Description)
                    .ToArray();
                return BadRequest(new ResponseError
                {
                    StatusCode = ((int)HttpStatusCode.BadRequest).ToString(),
                    Messages = errors
                });
            }
            return Created($"/users/{user.Id}", null);
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserAuthenticationResponse>> Login([FromBody] UserAuthenticationRequest authRequest)
        {
            int expiryInDays = int.Parse(configuration["AuthSettings:RefreshExpiryInDays"]);
            var token = await authService.LoginUserAsync(authRequest.Email, authRequest.Password, expiryInDays);
            var tokenDto = mapper.Map<AuthToken>(token);
            var user = await authService.GetUserByEmailAsync(authRequest.Email);
            var response = new UserAuthenticationResponse()
            {
                AuthToken = tokenDto,
                UserName = user.UserName,
                Email = user.Email
            };
            tokenDto.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(expiryInDays);
            return Ok(tokenDto);
        }
        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UserUpdateDataRequest updateRequest)
        {
            UserUpdateData serviceUpdateRequest = mapper.Map<UserUpdateData>(updateRequest);
            var identityErrors = await authService.UpdateUser(serviceUpdateRequest);
            if (identityErrors.Count > 0)
            {
                var errors = identityErrors.Where(e => !e.Description.Contains("Username")).Select(e => e.Description).ToArray();
                return BadRequest(new ResponseError { StatusCode = $"{(int)HttpStatusCode.BadRequest}", Messages = errors.ToArray() });
            }
            return Ok();
        }
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthToken>> Refresh([FromBody] AuthToken accessTokenDto)
        {
            AccessTokenData accessToken = mapper.Map<AccessTokenData>(accessTokenDto);
            var newToken = await authService.RefreshToken(accessToken);
            return Ok(mapper.Map<AuthToken>(newToken));
        }
    }
}