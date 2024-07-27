using ServerPulse.EventCommunication.Events;
using System.Text.Json;
using TestKafka.Consumer.Services;

namespace AnalyzerApi.Services
{
    public class MessageReceiver : IMessageReceiver
    {
        private readonly IMessageConsumer messageConsumer;
        private readonly string aliveTopic;
        private readonly string configurationTopic;
        private readonly string loadTopic;
        private readonly int timeoutInMilliseconds;

        public MessageReceiver(IMessageConsumer messageConsumer, IConfiguration configuration)
        {
            this.messageConsumer = messageConsumer;
            aliveTopic = configuration[Configuration.KAFKA_ALIVE_TOPIC]!;
            configurationTopic = configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]!;
            loadTopic = configuration[Configuration.KAFKA_LOAD_TOPIC]!;
            timeoutInMilliseconds = int.Parse(configuration[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]!);
        }

        public async Task<PulseEvent?> ReceiveLastPulseEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = aliveTopic.Replace("{id}", key);
            return await TaskGetLastEventFromTopic<PulseEvent>(topic, cancellationToken);
        }
        public async Task<ConfigurationEvent?> ReceiveLastConfigurationEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = configurationTopic.Replace("{id}", key);
            return await TaskGetLastEventFromTopic<ConfigurationEvent>(topic, cancellationToken);
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
        public async Task<int> ReceiveLoadEventAmountInDateRangeByKeyAsync(string key, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            string topic = loadTopic.Replace("{id}", key);
            int amount = await messageConsumer.GetAmountTopicMessagesInDateRangeAsync(topic, startDate, endDate, timeoutInMilliseconds, cancellationToken);
            return amount;
        }
    }
}