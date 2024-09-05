using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerPulse.Client.Services.Interfaces;
using ServerPulse.EventCommunication.Events;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ServerPulse.Client.Services
{
    public class QueueMessageSender<T> : BackgroundService, IQueueMessageSender<T> where T : BaseEvent
    {
        protected readonly IMessageSender messageSender;
        protected readonly ILogger<QueueMessageSender<T>> logger;
        protected readonly ConcurrentQueue<T> eventQueue = new ConcurrentQueue<T>();
        protected readonly EventSendingSettings<T> settings;

        public QueueMessageSender(IMessageSender messageSender, EventSendingSettings<T> settings, ILogger<QueueMessageSender<T>> logger)
        {
            this.messageSender = messageSender;
            this.logger = logger;
            this.settings = settings;
        }

        public virtual void SendEvent(T ev)
        {
            eventQueue.Enqueue(ev);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new(TimeSpan.FromSeconds(settings.EventSendingInterval));
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    if (!eventQueue.IsEmpty)
                    {
                        var json = GetEventsJson();
                        await messageSender.SendJsonAsync(json, settings.EventController, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occurred while sending {typeof(T).Name} events.");
                }
            }
        }
        protected virtual string GetEventsJson()
        {
            var events = new List<T>();
            for (int i = 0; i < settings.MaxEventSendingAmount && eventQueue.TryDequeue(out var ev); i++)
            {
                events.Add(ev);
            }
            return JsonSerializer.Serialize(events);
        }
    }
}