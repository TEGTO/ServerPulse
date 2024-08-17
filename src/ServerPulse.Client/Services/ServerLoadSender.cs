using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerPulse.EventCommunication.Events;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ServerPulse.Client.Services
{
    public class ServerLoadSender : BackgroundService, IServerLoadSender
    {
        private readonly IMessageSender messageSender;
        private readonly string sendingUrl;
        private readonly int maxEventsPerSending;
        private readonly double sendingInterval;
        private readonly ConcurrentQueue<LoadEvent> loadEvents = new ConcurrentQueue<LoadEvent>();
        private readonly ILogger<ServerLoadSender> logger;

        public ServerLoadSender(IMessageSender eventSender, ServerPulseSettings configuration, ILogger<ServerLoadSender> logger)
        {
            this.messageSender = eventSender;
            this.sendingUrl = configuration.EventController + $"/serverinteraction/load";
            this.maxEventsPerSending = configuration.MaxEventSendingAmount;
            this.sendingInterval = configuration.EventSendingInterval;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void SendEvent(LoadEvent loadEvent)
        {
            loadEvents.Enqueue(loadEvent);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new(TimeSpan.FromSeconds(sendingInterval));
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    if (!loadEvents.IsEmpty)
                    {
                        var json = GetEventsJson();
                        await messageSender.SendJsonAsync(json, sendingUrl, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while sending load events.");
                }
            }
        }
        private string GetEventsJson()
        {
            var events = new List<LoadEvent>();
            for (int i = 0; i < maxEventsPerSending && loadEvents.TryDequeue(out var loadEvent); i++)
            {
                events.Add(loadEvent);
            }
            return JsonSerializer.Serialize(events);
        }
    }
}