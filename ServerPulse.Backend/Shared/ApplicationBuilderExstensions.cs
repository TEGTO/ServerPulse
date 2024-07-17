using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Repositories;

namespace Shared
{
    public static class ApplicationBuilderExstensions
    {
        public static async Task<IApplicationBuilder> ConfigureDatabaseAsync<TContext>(this IApplicationBuilder builder, CancellationToken cancellationToken) where TContext : DbContext
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                var configuration = services.GetRequiredService<IConfiguration>();
                var logger = services.GetRequiredService<ILogger<IApplicationBuilder>>();
                var repository = services.GetRequiredService<IDatabaseRepository<TContext>>();
                try
                {
                    if (configuration["EFCreateDatabase"] == "true")
                    {
                        logger.LogInformation("Applying database migrations...");
                        await repository.MigrateDatabaseAsync(cancellationToken);
                        logger.LogInformation("Database migrations applied successfully.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }
            return builder;
        }
    }
}
