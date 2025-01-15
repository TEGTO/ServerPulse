using Castle.DynamicProxy;
using DatabaseControl.Repositories;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Polly;
using Polly.Registry;
using Proxies.Interceptors;
using Resilience;

namespace DatabaseControl
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositoryWithResilience<Context>(this IServiceCollection services, IConfiguration configuration) where Context : DbContext
        {
            var pipelineConfiguration = configuration
                .GetSection(DatabaseConfigurationKeys.REPOSITORY_RESILIENCE_PIPELINE)
                .Get<ResiliencePipelineSettings>() ?? new ResiliencePipelineSettings();

            services.AddResiliencePipeline(DatabaseConfigurationKeys.REPOSITORY_RESILIENCE_PIPELINE, (builder, context) =>
            {
                ResilienceHelpers.ConfigureResiliencePipeline(builder, context, pipelineConfiguration);
            });

            services.AddSingleton<DatabaseRepository<Context>>();
            services.TryAddSingleton<IProxyGenerator, ProxyGenerator>();

            services.AddSingleton(sp =>
            {
                var resiliencePipelineProvider = sp.GetRequiredService<ResiliencePipelineProvider<string>>();
                var resiliencePipeline = resiliencePipelineProvider.GetPipeline(DatabaseConfigurationKeys.REPOSITORY_RESILIENCE_PIPELINE);

                var repository = sp.GetRequiredService<DatabaseRepository<Context>>();

                var generator = sp.GetRequiredService<IProxyGenerator>();

                return generator.CreateInterfaceProxyWithTarget<IDatabaseRepository<Context>>(
                    repository,
                    new ResilienceInterceptor(resiliencePipeline)
                );
            });

            return services;
        }

        public static IServiceCollection AddDbContextFactory<Context>(
            this IServiceCollection services,
            string connectionString,
            string? migrationAssembly = null,
            Action<NpgsqlDbContextOptionsBuilder>? dbAdditionalConfig = null,
            Action<DbContextOptionsBuilder>? additionalConfig = null
        ) where Context : DbContext
        {
            services.AddDbContextFactory<Context>(options =>
            {
                var npgsqlOptions = options.UseNpgsql(connectionString, b =>
                {
                    if (!string.IsNullOrEmpty(migrationAssembly))
                    {
                        b.MigrationsAssembly(migrationAssembly);
                    }
                    dbAdditionalConfig?.Invoke(b);
                });

                options.UseSnakeCaseNamingConvention();
                npgsqlOptions.UseExceptionProcessor();

                additionalConfig?.Invoke(options);
            });

            return services;
        }
    }
}
