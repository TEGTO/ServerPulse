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

            services.AddSingleton<ServerLoadSender>();
            services.AddSingleton<IServerLoadSender>(sp => sp.GetRequiredService<ServerLoadSender>());
            services.AddHostedService(sp => sp.GetRequiredService<ServerLoadSender>());

            services.AddHostedService<ServerStatusSender>();

            return services;
        }
    }
}