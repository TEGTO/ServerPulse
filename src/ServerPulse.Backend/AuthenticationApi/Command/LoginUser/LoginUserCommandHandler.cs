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
        private readonly IUserService userService;
        private readonly IMapper mapper;

        public LoginUserCommandHandler(IAuthService authService, IUserService userService, IMapper mapper)
        {
            this.authService = authService;
            this.userService = userService;
            this.mapper = mapper;
        }

        public async Task<UserAuthenticationResponse> Handle(LoginUserCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var user = await userService.GetUserByLoginAsync(request.Login, cancellationToken);

            if (user == null) throw new UnauthorizedAccessException("Invalid authentication! Wrong password or login!");

            var loginModel = new LoginUserModel(user, request.Password);
            var token = await authService.LoginUserAsync(loginModel, cancellationToken);

            var tokenDto = mapper.Map<AuthToken>(token);

            return new UserAuthenticationResponse
            {
                AuthToken = tokenDto,
                Email = user.Email,
            };
        }
    }
}
