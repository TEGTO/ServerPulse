using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.Client.Services
{
    internal class ServerStatusSender : BackgroundService
    {
        private readonly IMessageSender messageSender;
        private readonly string pulseUrl;
        private readonly string confUrl;
        private readonly string key;
        private readonly double sendingInterval;
        private readonly ILogger<ServerLoadSender> logger;

        public ServerStatusSender(IMessageSender messageSender, ServerPulseSettings configuration, ILogger<ServerLoadSender> logger)
        {
            this.messageSender = messageSender;
            pulseUrl = configuration.EventController + $"/serverinteraction/pulse";
            confUrl = configuration.EventController + $"/serverinteraction/configuration";
            key = configuration.Key;
            sendingInterval = configuration.ServerKeepAliveInterval;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var confEvent = new ConfigurationEvent(key, TimeSpan.FromSeconds(sendingInterval));
                await messageSender.SendJsonAsync(confEvent.ToString(), confUrl, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while sending load events.");
            }

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(sendingInterval));
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var ev = new PulseEvent(key, true);
                    await messageSender.SendJsonAsync(ev.ToString(), pulseUrl, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while sending load events.");
                }
            }
        }
    }
}