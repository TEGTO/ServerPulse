using Microsoft.Extensions.Hosting;
using ServerPulse.Client.Events;
using System.Text.Json;

namespace ServerPulse.Client.Services
{
    public class ServerLoadSender : BackgroundService, IServerLoadSender
    {
        private readonly IMessageSender messageSender;
        private readonly string sendingUrl;
        private readonly int maxEventsPerSending;
        private readonly double sendingInterval;
        private Queue<LoadEvent> loadEvents = new Queue<LoadEvent>();

        public ServerLoadSender(IMessageSender eventSender, Configuration configuration)
        {
            this.messageSender = eventSender;
            sendingUrl = configuration.EventController + $"/serverinteraction/load";
            maxEventsPerSending = configuration.MaxEventSendingAmount;
            sendingInterval = configuration.EventSendingInterval;
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
                var json = GetEventsJson();
                await messageSender.SendJsonAsync(json, sendingUrl, stoppingToken);
            }
        }
        private string GetEventsJson()
        {
            var events = new List<LoadEvent>();
            for (int i = 0; i < maxEventsPerSending && loadEvents.Count > 0; i++)
            {
                events.Add(loadEvents.Dequeue());
            }
            return JsonSerializer.Serialize(events);
        }
    }
}