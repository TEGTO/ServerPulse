using AuthenticationApi.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationApi.BackgroundServices
{
    public class UnconfirmedUserCleanupService
    {
        private readonly UserManager<User> userManager;
        private readonly ILogger<UnconfirmedUserCleanupService> logger;
        private readonly int deleteUnconfirmedUserAfterMinutes;

        public UnconfirmedUserCleanupService(UserManager<User> userManager, ILogger<UnconfirmedUserCleanupService> logger, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.logger = logger;
            deleteUnconfirmedUserAfterMinutes = int.Parse(configuration[ConfigurationKeys.DELETE_UNCONRFIRMED_USERS_AFTER_MINUTES] ?? "60");
        }

        public async Task CleanupUnconfirmedUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await userManager.Users.Where(x => !x.EmailConfirmed).ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;

            foreach (var user in users)
            {
                if (user.RegisteredAtUtc.AddMinutes(deleteUnconfirmedUserAfterMinutes) < now)
                {
                    var result = await userManager.DeleteAsync(user);

                    if (!result.Succeeded)
                    {
                        var str = $"Failed to delete user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                        logger.LogWarning(str);
                    }
                }
            }
        }
    }
}
