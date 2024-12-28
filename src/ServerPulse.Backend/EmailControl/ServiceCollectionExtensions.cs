using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailControl
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SETTINGS_SECTION));

            services.AddSingleton<IEmailClientWrapper, EmailClientWrapper>();
            services.AddSingleton<IEmailSender, EmailSender>();

            return services;
        }
    }
}
