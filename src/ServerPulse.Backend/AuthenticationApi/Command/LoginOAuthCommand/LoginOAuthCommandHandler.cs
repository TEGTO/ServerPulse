using AuthenticationApi.Dtos;
using AuthenticationApi.Dtos.OAuth;
using AuthenticationApi.Services;
using AutoMapper;
using MediatR;

namespace AuthenticationApi.Command.LoginOAuthCommand
{
    public class LoginOAuthCommandHandler : IRequestHandler<LoginOAuthCommand, UserAuthenticationResponse>
    {
        private readonly Dictionary<OAuthLoginProvider, IOAuthService> oAuthServices;
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public LoginOAuthCommandHandler(Dictionary<OAuthLoginProvider, IOAuthService> oAuthServices, IAuthService authService, IMapper mapper)
        {
            this.oAuthServices = oAuthServices;
            this.authService = authService;
            this.mapper = mapper;
        }

        public async Task<UserAuthenticationResponse> Handle(LoginOAuthCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var loginModel = await oAuthServices[request.OAuthLoginProvider].GetProviderModelOnCodeAsync(
                new OAuthAccessCodeParams(request.Code, request.CodeVerifier, request.RedirectUrl), cancellationToken);

            if (loginModel == null)
            {
                throw new InvalidOperationException("Login model provider is null!");
            }

            var tokenData = await authService.LoginUserWithProviderAsync(loginModel, cancellationToken);

            var tokenDataDto = mapper.Map<AccessTokenDataDto>(tokenData);

            return new UserAuthenticationResponse
            {
                AuthToken = tokenDataDto,
                Email = loginModel.Email,
            };
        }
    }
}
