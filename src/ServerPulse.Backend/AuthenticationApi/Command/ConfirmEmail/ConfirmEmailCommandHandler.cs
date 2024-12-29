using AuthenticationApi.Dtos;
using AuthenticationApi.Services;
using AutoMapper;
using ExceptionHandling;
using MediatR;

namespace AuthenticationApi.Command.ConfirmEmail
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, UserAuthenticationResponse>
    {
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public ConfirmEmailCommandHandler(IAuthService authService, IMapper mapper)
        {
            this.authService = authService;
            this.mapper = mapper;
        }

        public async Task<UserAuthenticationResponse> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var result = await authService.ConfirmEmailAsync(request.Email, request.Token);

            if (Utilities.HasErrors(result.Errors, out var errorResponse))
            {
                throw new AuthorizationException(errorResponse);
            }

            var tokenData = await authService.LoginUserAfterConfirmationAsync(request.Email, cancellationToken);

            var tokenDataDto = mapper.Map<AccessTokenDataDto>(tokenData);

            return new UserAuthenticationResponse
            {
                AuthToken = tokenDataDto,
                Email = request.Email,
            };
        }
    }
}
