using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Configurations;
using Shared.Helpers;

namespace Shared
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedFluentValidation(this IServiceCollection services, Type type)
        {
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining(typeof(ServiceCollectionExtensions));
            services.AddValidatorsFromAssemblyContaining(type);
            ValidatorOptions.Global.LanguageManager.Enabled = false;
            return services;
        }
        public static IServiceCollection AddApplicationCors(this IServiceCollection services, IConfiguration configuration, string allowSpecificOrigins, bool isDevelopment)
        {
            var allowedOriginsString = configuration[SharedConfiguration.ALLOWED_CORS_ORIGINS] ?? string.Empty;
            var allowedOrigins = allowedOriginsString.Split(",", StringSplitOptions.RemoveEmptyEntries);

            services.AddCors(options =>
            {
                options.AddPolicy(name: allowSpecificOrigins, policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .AllowAnyMethod();
                    if (isDevelopment)
                    {
                        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
                    }
                });
            });
            return services;
        }
        public static IServiceCollection AddCustomHttpClientServiceWithResilience(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient(SharedConfiguration.HTTP_CLIENT_RESILIENCE_PIPELINE).AddStandardResilienceHandler();
            services.AddSingleton<IHttpHelper, HttpHelper>();
            return services;
        }
    }
}
