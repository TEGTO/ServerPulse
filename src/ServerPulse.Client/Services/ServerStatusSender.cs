﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.Client.Services
{
    internal sealed class ServerStatusSender : BackgroundService
    {
        private readonly IMessageSender messageSender;
        private readonly ILogger<ServerStatusSender> logger;
        private readonly SendingSettings<PulseEvent> pulseSettings;
        private readonly SendingSettings<ConfigurationEvent> configurationSettings;

        public ServerStatusSender(
            IMessageSender messageSender,
            SendingSettings<PulseEvent> pulseSettings,
            SendingSettings<ConfigurationEvent> configurationSettings,
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
                var confEvent = new ConfigurationEvent(configurationSettings.Key, TimeSpan.FromSeconds(configurationSettings.SendingInterval));
                await messageSender.SendJsonAsync(confEvent.ToString(), configurationSettings.SendingEndpoint, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while sending load events.");
            }

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(pulseSettings.SendingInterval));
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var ev = new PulseEvent(pulseSettings.Key, true);
                    await messageSender.SendJsonAsync(ev.ToString(), pulseSettings.SendingEndpoint, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while sending load events.");
                }
            }
        }
    }
}