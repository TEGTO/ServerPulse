using AuthData.Domain.Entities;
using AuthenticationApi.Domain.Dtos;
using AuthenticationApi.Domain.Models;
using AuthenticationApi.Services;
using AutoMapper;
using ExceptionHandling;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationApi.Command.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserAuthenticationResponse>
    {
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public RegisterUserCommandHandler(IAuthService authService, IMapper mapper)
        {
            this.authService = authService;
            this.mapper = mapper;
        }

        public async Task<UserAuthenticationResponse> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = mapper.Map<User>(request);

            var errors = new List<IdentityError>();

            var registerModel = new RegisterUserModel(user, request.Password);

            errors.AddRange((await authService.RegisterUserAsync(registerModel, cancellationToken)).Errors);
            if (Utilities.HasErrors(errors, out var errorResponse)) throw new AuthorizationException(errorResponse);

            var loginParams = new LoginUserModel(user, request.Password);
            var token = await authService.LoginUserAsync(loginParams, cancellationToken);

            var tokenDto = mapper.Map<AuthToken>(token);

            return new UserAuthenticationResponse
            {
                AuthToken = tokenDto,
                Email = user.Email,
                UserName = user.UserName
            };
        }
    }
}
