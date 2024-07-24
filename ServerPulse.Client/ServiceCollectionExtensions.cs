using Microsoft.Extensions.DependencyInjection;
using ServerPulse.Client.Services;

namespace ServerPulse.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServerPulseClient(this IServiceCollection services, Configuration configuration)
        {
            services.AddHttpClient();
            services.AddSingleton(configuration);
            services.AddSingleton<IEventSender, EventSender>();
            services.AddHostedService<AliveSender>();

            return services;
        }
    }
}