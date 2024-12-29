using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundTask
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureHangfireWthPostgreSql(this IServiceCollection services, string? connectionString)
        {
            services.AddHangfire(config => config.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));
            services.AddHangfireServer();
        }
    }
}
