using Microsoft.Extensions.DependencyInjection;
using ServerPulse.Client.Services;
using ServerPulse.Client.Services.Interfaces;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServerPulseClient(this IServiceCollection services, EventSendingSettings settings)
        {
            services.AddHttpClient();
            services.AddSingleton<IMessageSender, MessageSender>();

            services.AddSingleton(settings);
            services.AddSingleton(EventSendingSettings<PulseEvent>.CreateCustomSettings(settings, "/serverinteraction/pulse", settings.ServerKeepAliveInterval));
            services.AddSingleton(EventSendingSettings<ConfigurationEvent>.CreateCustomSettings(settings, "/serverinteraction/configuration", settings.ServerKeepAliveInterval));
            services.AddSingleton(EventSendingSettings<LoadEvent>.CreateCustomSettings(settings, "/serverinteraction/load", settings.EventSendingInterval));
            services.AddSingleton(EventSendingSettings<CustomEvent>.CreateCustomSettings(settings, "/serverinteraction/custom", settings.EventSendingInterval));

            services.AddSingleton<QueueMessageSender<LoadEvent>>();
            services.AddSingleton<IQueueMessageSender<LoadEvent>>(sp => sp.GetRequiredService<QueueMessageSender<LoadEvent>>());
            services.AddHostedService(sp => sp.GetRequiredService<QueueMessageSender<LoadEvent>>());

            services.AddSingleton<CustomEventSender>();
            services.AddSingleton<IQueueMessageSender<CustomEvent>>(sp => sp.GetRequiredService<CustomEventSender>());
            services.AddHostedService(sp => sp.GetRequiredService<CustomEventSender>());

            services.AddHostedService<ServerStatusSender>();

            return services;
        }
    }
}