using Microsoft.Extensions.Hosting;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.Client.Services
{
    internal class ServerStatusSender : BackgroundService
    {
        private readonly IMessageSender messageSender;
        private readonly string aliveUrl;
        private readonly string confUrl;
        private readonly string key;
        private readonly double sendingInterval;

        public ServerStatusSender(IMessageSender messageSender, Configuration configuration)
        {
            this.messageSender = messageSender;
            aliveUrl = configuration.EventController + $"/serverinteraction/alive";
            confUrl = configuration.EventController + $"/serverinteraction/configuration";
            key = configuration.Key;
            sendingInterval = configuration.ServerKeepAliveInterval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var confEvent = new ConfigurationEvent(key, TimeSpan.FromSeconds(sendingInterval));
            await messageSender.SendJsonAsync(confEvent.ToString(), confUrl, stoppingToken);

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(sendingInterval));
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                var aliveEvent = new AliveEvent(key, true);
                await messageSender.SendJsonAsync(aliveEvent.ToString(), aliveUrl, stoppingToken);
            }
        }
    }
}