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
        private readonly int expiryInDays;

        public AuthController(IMapper mapper, IAuthService authService, IConfiguration configuration)
        {
            this.mapper = mapper;
            this.authService = authService;
            this.configuration = configuration;
            this.expiryInDays = int.Parse(configuration["AuthSettings:RefreshExpiryInDays"]);
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
            var token = await authService.LoginUserAsync(authRequest.Email, authRequest.Password, expiryInDays);
            var tokenDto = mapper.Map<AuthToken>(token);
            tokenDto.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(expiryInDays);
            var user = await authService.GetUserByEmailAsync(authRequest.Email);
            var response = new UserAuthenticationResponse()
            {
                AuthToken = tokenDto,
                UserName = user.UserName,
                Email = user.Email
            };
            return Ok(response);
        }
        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UserUpdateDataRequest updateRequest)
        {
            UserUpdateData serviceUpdateRequest = mapper.Map<UserUpdateData>(updateRequest);
            var identityErrors = await authService.UpdateUserAsync(serviceUpdateRequest);
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
            var newToken = await authService.RefreshTokenAsync(accessToken);
            var tokenDto = mapper.Map<AuthToken>(newToken);
            tokenDto.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(expiryInDays);
            return Ok(tokenDto);
        }
        [HttpPost("check")]
        public async Task<ActionResult<CheckAuthDataResponse>> CheckAuthData([FromBody] CheckAuthDataRequest request)
        {
            var isCorrect = await authService.CheckAuthDataAsync(request.Email, request.Password);
            var checkAuthDataResponse = new CheckAuthDataResponse
            {
                IsCorrect = isCorrect
            };
            return Ok(checkAuthDataResponse);
        }
    }
}