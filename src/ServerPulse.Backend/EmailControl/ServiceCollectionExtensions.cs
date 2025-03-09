using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EmailControl
{
    public enum EmailServiceType
    {
        AWS, Azure
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "<Pending>")]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailService(this IServiceCollection services,
            IConfiguration configuration,
            EmailServiceType serviceType = EmailServiceType.AWS,
            bool enableFeature = false)
        {
            if (enableFeature)
            {
                switch (serviceType)
                {
                    case EmailServiceType.AWS:
                        RegisterAwsEmail(services, configuration);
                        break;
                    case EmailServiceType.Azure:
                        RegisterAzureEmail(services, configuration);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                RegisterMockEmail(services);
            }

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

        private static void RegisterMockEmail(IServiceCollection services)
        {
            var mock = new Mock<IEmailSender>();
            services.AddSingleton(mock.Object);
        }
    }
}