using AuthenticationApi.Application.BackgroundServices;
using Hangfire;
using ApplicationKeys = AuthenticationApi.Application.ConfigurationKeys;

namespace AuthenticationApi
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder ConfigureRecurringJobs(this IApplicationBuilder builder, IConfiguration configuration)
        {
            var useClenup = bool.Parse(configuration[ApplicationKeys.USE_USER_UNCONFIRMED_CLEANUP] ?? "false");
            if (useClenup)
            {
                var intervalInMinutes = float.Parse(configuration[ApplicationKeys.UNCONRFIRMED_USERS_CLEANUP_IN_MINUTES] ?? "60");

                RecurringJob.AddOrUpdate<UnconfirmedUserCleanupService>(
                    "CleanupUnconfirmedUsers",
                    service => service.CleanupUnconfirmedUsersAsync(CancellationToken.None),
                    GetCronExpressionForAnyInterval(intervalInMinutes)
                );
            }

            return builder;
        }

        private static string GetCronExpressionForAnyInterval(float intervalInMinutes)
        {
            int totalSeconds = (int)(intervalInMinutes * 60);

            if (totalSeconds < 60)
            {
                return $"*/{totalSeconds} * * * * *";
            }

            int minutes = totalSeconds / 60;
            if (minutes <= 59)
            {
                return $"*/{minutes} * * * *";
            }

            int hours = minutes / 60;
            int remainingMinutes = minutes % 60;

            if (remainingMinutes == 0)
            {
                return $"0 */{hours} * * *";
            }
            else
            {
                return $"{remainingMinutes} */{hours} * * *";
            }
        }
    }
}
