using AnalyzerApi.Services.Interfaces;
using Confluent.Kafka;
using ServerPulse.EventCommunication.Events;
using System.Runtime.CompilerServices;
using System.Text.Json;
using TestKafka.Consumer.Services;

namespace AnalyzerApi.Services
{
    public class ServerLoadReceiver : IServerLoadReceiver
    {
        private readonly IMessageConsumer messageConsumer;
        private readonly string loadTopic;
        private readonly int timeoutInMilliseconds;

        public ServerLoadReceiver(IMessageConsumer messageConsumer, IConfiguration configuration)
        {
            this.messageConsumer = messageConsumer;
            loadTopic = configuration[Configuration.KAFKA_LOAD_TOPIC]!;
            timeoutInMilliseconds = int.Parse(configuration[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]!);
        }

        public async IAsyncEnumerable<LoadEvent> ConsumeLoadEventAsync(string key, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string topic = loadTopic.Replace("{id}", key);
            await foreach (var message in messageConsumer.ConsumeAsync(topic, timeoutInMilliseconds, Offset.End, cancellationToken))
            {
                var loadEvent = JsonSerializer.Deserialize<LoadEvent>(message);
                if (loadEvent != null)
                {
                    yield return loadEvent;
                }
            }
        }
        public async Task<IEnumerable<LoadEvent>> ReceiveEventsInRangeAsync(string key, DateTime from, DateTime to, CancellationToken cancellationToken)
        {
            string topic = loadTopic.Replace("{id}", key);
            List<string> events = await messageConsumer.ReadMessagesInDateRangeAsync(topic, from, to, timeoutInMilliseconds, cancellationToken);
            List<LoadEvent> loadEvents = new List<LoadEvent>();
            foreach (var eventString in events)
            {
                var loadEvent = JsonSerializer.Deserialize<LoadEvent>(eventString);
                if (loadEvent != null)
                {
                    loadEvents.Add(loadEvent);
                }
            }
            return loadEvents;
        }
        public async Task<LoadEvent?> ReceiveLastLoadEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = loadTopic.Replace("{id}", key);
            return await TaskGetLastEventFromTopic<LoadEvent>(topic, cancellationToken);
        }
        private async Task<T?> TaskGetLastEventFromTopic<T>(string topic, CancellationToken cancellationToken) where T : BaseEvent
        {
            string? message = await messageConsumer.ReadLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);
            if (!string.IsNullOrEmpty(message))
            {
                return JsonSerializer.Deserialize<T>(message);
            }
            return null;
        }
        public async Task<int> ReceiveLoadEventAmountByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = loadTopic.Replace("{id}", key);
            int amount = await messageConsumer.GetAmountTopicMessagesAsync(topic, timeoutInMilliseconds, cancellationToken);
            return amount;
        }
    }
}