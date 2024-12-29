using EventCommunication;
using Microsoft.Extensions.DependencyInjection;
using ServerPulse.Client.Services;
using ServerPulse.Client.Services.Interfaces;

namespace ServerPulse.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServerPulseClient(this IServiceCollection services, SendingSettings settings)
        {
            services.AddHttpClient();
            services.AddSingleton<IMessageSender, MessageSender>();

            services.AddSingleton(settings);
            services.AddSingleton(SendingSettings<PulseEvent>.CreateCustomSettings(settings, "/serverinteraction/pulse", settings.ServerKeepAliveInterval));
            services.AddSingleton(SendingSettings<ConfigurationEvent>.CreateCustomSettings(settings, "/serverinteraction/configuration", settings.ServerKeepAliveInterval));
            services.AddSingleton(SendingSettings<LoadEvent>.CreateCustomSettings(settings, "/serverinteraction/load", settings.SendingInterval));
            services.AddSingleton(SendingSettings<CustomEventContainer>.CreateCustomSettings(settings, "/serverinteraction/custom", settings.SendingInterval));

            services.AddSingleton<QueueMessageSender<LoadEvent>>();
            services.AddSingleton<IQueueMessageSender<LoadEvent>>(sp => sp.GetRequiredService<QueueMessageSender<LoadEvent>>());
            services.AddHostedService(sp => sp.GetRequiredService<QueueMessageSender<LoadEvent>>());

            services.AddSingleton<QueueMessageSender<CustomEventContainer>>();
            services.AddSingleton<IQueueMessageSender<CustomEventContainer>>(sp => sp.GetRequiredService<QueueMessageSender<CustomEventContainer>>());
            services.AddHostedService(sp => sp.GetRequiredService<QueueMessageSender<CustomEventContainer>>());

            services.AddHostedService<ServerStatusSender>();

            return services;
        }
    }
}