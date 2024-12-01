using DatabaseControl.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DatabaseControl
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task<IApplicationBuilder> ConfigureDatabaseAsync<TContext>(this IApplicationBuilder builder, CancellationToken cancellationToken) where TContext : DbContext
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<IApplicationBuilder>>();
                var repository = services.GetRequiredService<IDatabaseRepository<TContext>>();
                try
                {
                    logger.LogInformation("Applying database migrations...");
                    await repository.MigrateDatabaseAsync(cancellationToken);
                    logger.LogInformation("Database migrations applied successfully.");
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
