using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using AuthenticationApi.Infrastructure;

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

        public async Task<List<IdentityError>> UpdateUserAsync(User user, UserUpdateModel updateModel, bool resetPassword, CancellationToken cancellationToken)
        {
            List<IdentityError> identityErrors = new List<IdentityError>();

            if (updateModel.UserName != null && !updateModel.UserName.Equals(user.UserName))
            {
                var result = await userManager.SetUserNameAsync(user, updateModel.UserName);
                identityErrors.AddRange(result.Errors);
            }

            if (updateModel.NewEmail != null && !updateModel.NewEmail.Equals(user.Email))
            {
                var token = await userManager.GenerateChangeEmailTokenAsync(user, updateModel.NewEmail);
                var result = await userManager.ChangeEmailAsync(user, updateModel.NewEmail, token);
                identityErrors.AddRange(result.Errors);
            }

            if (!string.IsNullOrEmpty(updateModel.NewPassword) && !string.IsNullOrEmpty(updateModel.OldPassword))
            {
                if (resetPassword)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await userManager.ResetPasswordAsync(user, token, updateModel.NewPassword);
                    identityErrors.AddRange(result.Errors);
                }
                else
                {
                    var result = await userManager.ChangePasswordAsync(user, updateModel.OldPassword, updateModel.NewPassword);
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

        private static List<IdentityError> RemoveDuplicates(List<IdentityError> identityErrors)
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
