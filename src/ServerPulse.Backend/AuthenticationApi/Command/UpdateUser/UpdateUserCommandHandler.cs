using AuthenticationApi.Infrastructure;
using AuthenticationApi.Services;
using AutoMapper;
using ExceptionHandling;
using MediatR;

namespace AuthenticationApi.Command
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
    {
        private readonly IUserService userService;
        private readonly IMapper mapper;

        public UpdateUserCommandHandler(IUserService userService, IMapper mapper)
        {
            this.userService = userService;
            this.mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            var updateModel = mapper.Map<UserUpdateModel>(command.Request);
            var user = await userService.GetUserAsync(command.UserPrincipal, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException("User to update is not found!");
            }

            var identityErrors = await userService.UpdateUserAsync(user, updateModel, false, cancellationToken);
            if (Utilities.HasErrors(identityErrors, out var errorResponse)) throw new AuthorizationException(errorResponse);

            return Unit.Value;
        }
    }
}
