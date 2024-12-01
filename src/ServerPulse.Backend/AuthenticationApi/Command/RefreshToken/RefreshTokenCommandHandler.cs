using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Services;
using AutoMapper;
using MediatR;

namespace AuthenticationApi.Command.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthToken>
    {
        private readonly IAuthService authService;
        private readonly ITokenService tokenService;
        private readonly IUserService userService;
        private readonly IMapper mapper;

        public RefreshTokenCommandHandler(IAuthService authService, IUserService userService, ITokenService tokenService, IMapper mapper)
        {
            this.authService = authService;
            this.tokenService = tokenService;
            this.userService = userService;
            this.mapper = mapper;
        }

        public async Task<AuthToken> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var tokenData = mapper.Map<AccessTokenData>(command.Request);

            var principal = tokenService.GetPrincipalFromToken(tokenData.AccessToken);

            var user = await userService.GetUserAsync(principal, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException("User is not found by access token!");
            }

            var refreshModel = new RefreshTokenModel(user, tokenData);
            var newToken = await authService.RefreshTokenAsync(refreshModel, cancellationToken);

            return mapper.Map<AuthToken>(newToken);
        }
    }
}
