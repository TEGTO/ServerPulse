using Authentication.Models;
using AuthenticationApi.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationApi.Services
{
    public interface IAuthService
    {
        public Task<AccessTokenData> LoginUserAsync(LoginUserModel loginModel, CancellationToken cancellationToken);
        public Task<AccessTokenData> RefreshTokenAsync(RefreshTokenModel refreshTokenModel, CancellationToken cancellationToken);
        public Task<IdentityResult> RegisterUserAsync(RegisterUserModel registerModel, CancellationToken cancellationToken);
    }
}