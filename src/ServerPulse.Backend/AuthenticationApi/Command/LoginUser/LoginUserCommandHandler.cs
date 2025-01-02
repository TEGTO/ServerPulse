using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using MediatR;
using Microsoft.FeatureManagement;

namespace AuthenticationApi.Command.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, UserAuthenticationResponse>
    {
        private readonly IAuthService authService;
        private readonly IFeatureManager featureManager;
        private readonly IMapper mapper;

        public LoginUserCommandHandler(IAuthService authService, IFeatureManager featureManager, IMapper mapper)
        {
            this.authService = authService;
            this.featureManager = featureManager;
            this.mapper = mapper;
        }

        public async Task<UserAuthenticationResponse> Handle(LoginUserCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            if (!await CheckEmailConfirmation(request))
            {
                throw new UnauthorizedAccessException("Invalid login or password.");
            }

            var loginModel = new LoginUserModel { Login = request.Login, Password = request.Password };
            var tokenData = await authService.LoginUserAsync(loginModel, cancellationToken);

            var tokenDataDto = mapper.Map<AccessTokenDataDto>(tokenData);

            return new UserAuthenticationResponse
            {
                AuthToken = tokenDataDto,
                Email = request.Login,
            };
        }

        private async Task<bool> CheckEmailConfirmation(UserAuthenticationRequest request)
        {
            if (await featureManager.IsEnabledAsync(Features.EMAIL_CONFIRMATION)
                && !await authService.CheckEmailConfirmationAsync(request.Login))
            {
                return false;
            }
            return true;
        }
    }
}
