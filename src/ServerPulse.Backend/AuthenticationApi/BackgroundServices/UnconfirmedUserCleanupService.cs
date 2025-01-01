using AuthenticationApi.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace AuthenticationApi.BackgroundServices
{
    public class UnconfirmedUserCleanupService
    {
        private readonly UserManager<User> userManager;
        private readonly IFeatureManager featureManager;
        private readonly ILogger<UnconfirmedUserCleanupService> logger;
        private readonly int cleanUpInMinutes;

        public UnconfirmedUserCleanupService(UserManager<User> userManager, IFeatureManager featureManager, ILogger<UnconfirmedUserCleanupService> logger, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.featureManager = featureManager;
            this.logger = logger;
            cleanUpInMinutes = int.Parse(configuration[ConfigurationKeys.UNCONRFIRMED_USERS_CLEANUP_IN_MINUTES] ?? "60");
        }

        public async Task CleanupUnconfirmedUsersAsync(CancellationToken cancellationToken = default)
        {
            if (await featureManager.IsEnabledAsync(Features.EMAIL_CONFIRMATION))
            {
                var users = await userManager.Users.Where(x => !x.EmailConfirmed).ToListAsync(cancellationToken);

                var now = DateTime.UtcNow;

                foreach (var user in users)
                {
                    if (user.RegisteredAtUtc.AddMinutes(cleanUpInMinutes) < now)
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
}
