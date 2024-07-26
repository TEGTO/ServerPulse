using Microsoft.Extensions.Hosting;
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
                if (!loadEvents.IsEmpty)
                {
                    var json = GetEventsJson();
                    await messageSender.SendJsonAsync(json, sendingUrl, stoppingToken);
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