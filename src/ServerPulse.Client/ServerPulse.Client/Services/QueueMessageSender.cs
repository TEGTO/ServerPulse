using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerPulse.Client.Services.Interfaces;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ServerPulse.Client.Services
{
    public class QueueMessageSender<T> : BackgroundService, IQueueMessageSender<T> where T : class
    {
        protected readonly IMessageSender messageSender;
        protected readonly ILogger<QueueMessageSender<T>> logger;
        protected readonly ConcurrentQueue<T> eventQueue = new ConcurrentQueue<T>();
        protected readonly SendingSettings<T> settings;

        public QueueMessageSender(IMessageSender messageSender, SendingSettings<T> settings, ILogger<QueueMessageSender<T>> logger)
        {
            this.messageSender = messageSender;
            this.logger = logger;
            this.settings = settings;
        }

        public virtual void SendMessage(T message)
        {
            eventQueue.Enqueue(message);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new(TimeSpan.FromSeconds(settings.SendingInterval));
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    if (!eventQueue.IsEmpty)
                    {
                        var json = GetEventsJson();
                        await messageSender.SendJsonAsync(json, settings.SendingEndpoint, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    var str = $"An error occurred while sending {typeof(T).Name} events.";
                    logger.LogError(ex, str);
                }
            }
        }

        protected virtual string GetEventsJson()
        {
            var events = new List<T>();
            for (int i = 0; i < settings.MaxMessageSendingAmount && eventQueue.TryDequeue(out var ev); i++)
            {
                events.Add(ev);
            }

            return JsonSerializer.Serialize(events);
        }
    }
}