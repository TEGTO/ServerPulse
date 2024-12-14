using AuthenticationApi.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthenticationApi.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> userManager;

        public UserService(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        #region IUserService Members

        public async Task<User?> GetUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
        {
            var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(id) ? null : await userManager.FindByIdAsync(id!);
        }

        public async Task<User?> GetUserByLoginAsync(string login, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(login);
            user = user == null ? await userManager.FindByNameAsync(login) : user;
            user = user == null ? await userManager.FindByIdAsync(login) : user;
            return user;
        }

        public async Task<IEnumerable<IdentityError>> UpdateUserAsync(User user, UserUpdateModel updateModel, bool resetPassword, CancellationToken cancellationToken)
        {
            var identityErrors = new List<IdentityError>();

            if (!string.IsNullOrEmpty(updateModel.UserName) && !updateModel.UserName.Equals(user.UserName))
            {
                var result = await userManager.SetUserNameAsync(user, updateModel.UserName);
                identityErrors.AddRange(result.Errors);
            }

            if (!string.IsNullOrEmpty(updateModel.Email) && !updateModel.Email.Equals(user.Email))
            {
                var token = await userManager.GenerateChangeEmailTokenAsync(user, updateModel.Email);
                var result = await userManager.ChangeEmailAsync(user, updateModel.Email, token);
                identityErrors.AddRange(result.Errors);
            }

            if (!string.IsNullOrEmpty(updateModel.Password) && !string.IsNullOrEmpty(updateModel.OldPassword))
            {
                if (resetPassword)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await userManager.ResetPasswordAsync(user, token, updateModel.Password);
                    identityErrors.AddRange(result.Errors);
                }
                else
                {
                    var result = await userManager.ChangePasswordAsync(user, updateModel.OldPassword, updateModel.Password);
                    identityErrors.AddRange(result.Errors);
                }
            }

            return RemoveDuplicates(identityErrors);
        }

        public async Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken)
        {
            return await userManager.CheckPasswordAsync(user, password);
        }

        #endregion

        #region Private Helpers

        private static IEnumerable<IdentityError> RemoveDuplicates(List<IdentityError> identityErrors)
        {
            identityErrors = identityErrors
            .GroupBy(e => e.Description)
            .Select(g => g.First())
            .ToList();
            return identityErrors;
        }

        #endregion
    }
}
