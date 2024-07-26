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
            services.AddSingleton<IMessageSender, MessageSender>();
            services.AddSingleton<IServerLoadSender, ServerLoadSender>();
            services.AddHostedService<ServerStatusSender>();
            services.AddHostedService<ServerLoadSender>();

            return services;
        }
    }
}