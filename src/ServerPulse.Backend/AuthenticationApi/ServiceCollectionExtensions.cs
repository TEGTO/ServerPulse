using AuthenticationApi.BackgroundServices;
using Hangfire;

namespace AuthenticationApi
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureRecurringJobs(this IApplicationBuilder builder, IConfiguration configuration)
        {
            var intervalInMinutes = float.Parse(configuration[ConfigurationKeys.UNCONRFIRMED_USERS_CLEANUP_IN_MINUTES] ?? "60");

            RecurringJob.AddOrUpdate<UnconfirmedUserCleanupService>(
                "CleanupUnconfirmedUsers",
                service => service.CleanupUnconfirmedUsersAsync(CancellationToken.None),
                GetCronExpressionForAnyInterval(intervalInMinutes)
            );
        }

        private static string GetCronExpressionForAnyInterval(float intervalInMinutes)
        {
            if (intervalInMinutes <= 59 && intervalInMinutes >= 1)
            {
                return $"*/{intervalInMinutes} * * * *";
            }
            else if (intervalInMinutes < 1)
            {
                int seconds = (int)(60f * intervalInMinutes);
                return $"*/{seconds} * * * * *";
            }

            int hours = (int)(intervalInMinutes / 60f);
            int minutes = (int)(intervalInMinutes % 60f);

            if (minutes == 0)
            {
                return $"0 */{hours} * * *";
            }
            else
            {
                return $"{minutes} */{hours} * * *";
            }
        }
    }
}
