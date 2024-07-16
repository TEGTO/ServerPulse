using Authentication.Models;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationApi.Services
{
    public interface IAuthService
    {
        public Task<AccessTokenData> LoginUserAsync(string login, string password, int refreshTokeExpiryInDays);
        public Task<User?> GetUserByLoginAsync(string login);
        public Task<AccessTokenData> RefreshTokenAsync(AccessTokenData accessTokenData);
        public Task<IdentityResult> RegisterUserAsync(User user, string password);
        public Task<List<IdentityError>> UpdateUserAsync(UserUpdateData updateData);
        public Task<bool> CheckAuthDataAsync(string login, string password);
    }
}