using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.Client.Services
{
    internal sealed class ServerStatusSender : BackgroundService
    {
        private readonly IMessageSender messageSender;
        private readonly ILogger<ServerStatusSender> logger;
        private readonly EventSendingSettings<PulseEvent> pulseSettings;
        private readonly EventSendingSettings<ConfigurationEvent> configurationSettings;

        public ServerStatusSender(
            IMessageSender messageSender,
            EventSendingSettings<PulseEvent> pulseSettings,
            EventSendingSettings<ConfigurationEvent> configurationSettings,
            ILogger<ServerStatusSender> logger)
        {
            this.messageSender = messageSender;
            this.logger = logger;
            this.pulseSettings = pulseSettings;
            this.configurationSettings = configurationSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var confEvent = new ConfigurationEvent(configurationSettings.Key, TimeSpan.FromSeconds(configurationSettings.EventSendingInterval));
                await messageSender.SendJsonAsync(confEvent.ToString(), configurationSettings.EventController, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while sending load events.");
            }

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(pulseSettings.EventSendingInterval));
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var ev = new PulseEvent(pulseSettings.Key, true);
                    await messageSender.SendJsonAsync(ev.ToString(), pulseSettings.EventController, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while sending load events.");
                }
            }
        }
    }
}