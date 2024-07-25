using Microsoft.Extensions.Hosting;
using ServerPulse.Client.Events;

namespace ServerPulse.Client.Services
{
    internal class ServerStatusSender : BackgroundService
    {
        private readonly IEventSender eventSender;
        private readonly Configuration configuration;
        private readonly string aliveUrl;
        private readonly string confUrl;

        public ServerStatusSender(IEventSender eventSender, Configuration configuration)
        {
            this.eventSender = eventSender;
            this.configuration = configuration;
            aliveUrl = configuration.EventController + $"/alive/{configuration.SlotKey}";
            confUrl = configuration.EventController + $"/configuration/{configuration.SlotKey}";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var confEvent = new ConfigurationEvent(configuration.SlotKey, TimeSpan.FromSeconds(configuration.AliveEventSendInterval));
            await eventSender.SendEventAsync(confEvent, confUrl, stoppingToken);

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(configuration.AliveEventSendInterval));
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                var aliveEvent = new AliveEvent(configuration.SlotKey, true);
                await eventSender.SendEventAsync(aliveEvent, aliveUrl, stoppingToken);
            }
        }
    }
}