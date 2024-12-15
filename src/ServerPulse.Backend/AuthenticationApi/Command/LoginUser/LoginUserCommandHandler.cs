using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Services;
using AutoMapper;
using MediatR;

namespace AuthenticationApi.Command.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, UserAuthenticationResponse>
    {
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public LoginUserCommandHandler(IAuthService authService, IMapper mapper)
        {
            this.authService = authService;
            this.mapper = mapper;
        }

        public async Task<UserAuthenticationResponse> Handle(LoginUserCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var loginModel = new LoginUserModel { Login = request.Login, Password = request.Password };
            var token = await authService.LoginUserAsync(loginModel, cancellationToken);

            var tokenDto = mapper.Map<AccessTokenDataDto>(token);

            return new UserAuthenticationResponse
            {
                AuthToken = tokenDto,
                Email = request.Login,
            };
        }
    }
}
