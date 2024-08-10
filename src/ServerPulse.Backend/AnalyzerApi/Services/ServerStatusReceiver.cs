using AnalyzerApi.Services.Interfaces;
using Confluent.Kafka;
using MessageBus.Interfaces;
using ServerPulse.EventCommunication.Events;
using Shared;
using System.Runtime.CompilerServices;

namespace AnalyzerApi.Services
{
    public class ServerStatusReceiver : IServerStatusReceiver
    {
        private readonly IMessageConsumer messageConsumer;
        private readonly string aliveTopic;
        private readonly string configurationTopic;
        private readonly int timeoutInMilliseconds;

        public ServerStatusReceiver(IMessageConsumer messageConsumer, IConfiguration configuration)
        {
            this.messageConsumer = messageConsumer;
            aliveTopic = configuration[Configuration.KAFKA_ALIVE_TOPIC]!;
            configurationTopic = configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]!;
            timeoutInMilliseconds = int.Parse(configuration[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]!);
        }

        public async IAsyncEnumerable<PulseEvent> ConsumePulseEventAsync(string key, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string topic = GetAliveTopic(key);
            await foreach (var message in messageConsumer.ConsumeAsync(topic, timeoutInMilliseconds, Offset.End, cancellationToken))
            {
                if (message.TryToDeserialize(out PulseEvent pulseEvent))
                {
                    yield return pulseEvent;
                }
            }
        }
        public async IAsyncEnumerable<ConfigurationEvent> ConsumeConfigurationEventAsync(string key, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string topic = GetConfigurationTopic(key);
            await foreach (var message in messageConsumer.ConsumeAsync(topic, timeoutInMilliseconds, Offset.End, cancellationToken))
            {
                if (message.TryToDeserialize(out ConfigurationEvent confEvent))
                {
                    yield return confEvent;
                }
            }
        }
        public async Task<PulseEvent?> ReceiveLastPulseEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetAliveTopic(key);
            return await TaskGetLastEventFromTopic<PulseEvent>(topic, cancellationToken);
        }
        public async Task<ConfigurationEvent?> ReceiveLastConfigurationEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetConfigurationTopic(key);
            return await TaskGetLastEventFromTopic<ConfigurationEvent>(topic, cancellationToken);
        }
        private async Task<T?> TaskGetLastEventFromTopic<T>(string topic, CancellationToken cancellationToken) where T : BaseEvent
        {
            string? message = await messageConsumer.ReadLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);
            if (message.TryToDeserialize(out T ev))
            {
                return ev;
            }
            return null;
        }

        private string GetAliveTopic(string key)
        {
            return aliveTopic + key;
        }
        private string GetConfigurationTopic(string key)
        {
            return configurationTopic + key;
        }
    }
}