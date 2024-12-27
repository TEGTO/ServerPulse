using FluentValidation;
using FluentValidation.AspNetCore;
using Helper;
using Helper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Shared.Configurations;

namespace Shared
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedFluentValidation(this IServiceCollection services, params Type[] types)
        {
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining(typeof(ServiceCollectionExtensions));

            foreach (var type in types)
            {
                services.AddValidatorsFromAssemblyContaining(type);
            }

            ValidatorOptions.Global.LanguageManager.Enabled = false;

            return services;
        }

        public static IServiceCollection AddHttpClientHelperServiceWithResilience(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient(HelperConfiguration.HTTP_CLIENT_HELPER).AddStandardResilienceHandler();
            services.AddSingleton<IHttpHelper, HttpHelper>();
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

        public static IServiceCollection AddSwagger(this IServiceCollection services, string title)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = title,
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });

            return services;
        }
    }
}
