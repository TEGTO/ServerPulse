using AuthData.Domain.Entities;
using AuthenticationApi.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthenticationApi.Services
{
    public interface IUserService
    {
        public Task<User?> GetUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
        public Task<User?> GetUserByLoginAsync(string login, CancellationToken cancellationToken);
        public Task<List<IdentityError>> UpdateUserAsync(User user, UserUpdateModel updateModel, bool resetPassword, CancellationToken cancellationToken);
        public Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken);
    }
}