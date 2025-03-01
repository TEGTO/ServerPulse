using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailControl
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "<Pending>")]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterAwsEmail(services, configuration);

            return services;
        }

        private static void RegisterAzureEmail(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureEmailSettings>(configuration.GetSection(AzureEmailSettings.SETTINGS_SECTION));

            services.AddSingleton<IAzureEmailClientFacade, AzureEmailClientFacade>();
            services.AddSingleton<IEmailSender, AzureEmailSender>();
        }

        private static void RegisterAwsEmail(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AwsEmailSettings>(configuration.GetSection(AwsEmailSettings.SETTINGS_SECTION));

            services.AddSingleton<IAwsEmailClientFacade, AwsEmailClientFacade>();
            services.AddSingleton<IEmailSender, AwsEmailSender>();
        }
    }
}