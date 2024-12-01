using AuthenticationApi.Services;
using MediatR;
using Shared.Dtos.Auth;

namespace AuthenticationApi.Command.ChechAuthData
{
    public class CheckAuthDataCommandHandler : IRequestHandler<CheckAuthDataCommand, CheckAuthDataResponse>
    {
        private readonly IUserService userService;

        public CheckAuthDataCommandHandler(IUserService userService)
        {
            this.userService = userService;
        }

        public async Task<CheckAuthDataResponse> Handle(CheckAuthDataCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = await userService.GetUserByLoginAsync(request.Login, cancellationToken);

            if (user == null)
            {
                return new CheckAuthDataResponse
                {
                    IsCorrect = false
                };
            }

            var isCorrect = await userService.CheckPasswordAsync(user, request.Password, cancellationToken);
            return new CheckAuthDataResponse
            {
                IsCorrect = isCorrect
            };
        }
    }
}
