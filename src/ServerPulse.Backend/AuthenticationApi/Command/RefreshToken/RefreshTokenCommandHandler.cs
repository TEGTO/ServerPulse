using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Services;
using AutoMapper;
using MediatR;

namespace AuthenticationApi.Command.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AccessTokenDataDto>
    {
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public RefreshTokenCommandHandler(IAuthService authService, IMapper mapper)
        {
            this.authService = authService;
            this.mapper = mapper;
        }

        public async Task<AccessTokenDataDto> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var tokenData = mapper.Map<AccessTokenData>(command.Request);

            var newToken = await authService.RefreshTokenAsync(tokenData, cancellationToken);

            return mapper.Map<AccessTokenDataDto>(newToken);
        }
    }
}
