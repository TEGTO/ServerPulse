using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using ExceptionHandling;
using MediatR;

namespace AuthenticationApi.Command
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
    {
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public UpdateUserCommandHandler(IAuthService authService, IMapper mapper)
        {
            this.authService = authService;
            this.mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var updateModel = mapper.Map<UserUpdateModel>(request);

            var errors = await authService.UpdateUserAsync(command.UserPrincipal, updateModel, false, cancellationToken);
            if (Utilities.HasErrors(errors, out var errorResponse)) throw new AuthorizationException(errorResponse);

            return Unit.Value;
        }
    }
}
