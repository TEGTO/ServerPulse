using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using ExceptionHandling;
using MediatR;

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

            var registerModel = new RegisterUserModel { User = user, Password = request.Password };

            var errors = (await authService.RegisterUserAsync(registerModel, cancellationToken)).Errors;
            if (Utilities.HasErrors(errors, out var errorResponse)) throw new AuthorizationException(errorResponse);

            var loginModel = new LoginUserModel { Login = request.Email, Password = request.Password };
            var token = await authService.LoginUserAsync(loginModel, cancellationToken);

            var tokenDto = mapper.Map<AccessTokenDataDto>(token);

            return new UserAuthenticationResponse
            {
                AuthToken = tokenDto,
                Email = user.Email,
            };
        }
    }
}
