using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using Confluent.Kafka;
using MessageBus.Interfaces;
using ServerPulse.EventCommunication.Events;
using System.Runtime.CompilerServices;

namespace AnalyzerApi.Services
{
    public class ServerStatusReceiver : IServerStatusReceiver
    {
        private readonly IMessageConsumer messageConsumer;
        private readonly IMapper mapper;
        private readonly string aliveTopic;
        private readonly string configurationTopic;
        private readonly int timeoutInMilliseconds;

        public ServerStatusReceiver(IMessageConsumer messageConsumer, IMapper mapper, IConfiguration configuration)
        {
            this.messageConsumer = messageConsumer;
            this.mapper = mapper;
            aliveTopic = configuration[Configuration.KAFKA_ALIVE_TOPIC]!;
            configurationTopic = configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]!;
            timeoutInMilliseconds = int.Parse(configuration[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]!);
        }

        public async IAsyncEnumerable<PulseEventWrapper> ConsumePulseEventAsync(string key, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string topic = GetAliveTopic(key);
            await foreach (var response in messageConsumer.ConsumeAsync(topic, timeoutInMilliseconds, Offset.End, cancellationToken))
            {
                if (response.TryDeserializeEventWrapper<PulseEvent, PulseEventWrapper>(mapper, out PulseEventWrapper ev))
                {
                    yield return ev;
                }
            }
        }
        public async IAsyncEnumerable<ConfigurationEventWrapper> ConsumeConfigurationEventAsync(string key, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string topic = GetConfigurationTopic(key);
            await foreach (var response in messageConsumer.ConsumeAsync(topic, timeoutInMilliseconds, Offset.End, cancellationToken))
            {
                if (response.TryDeserializeEventWrapper<ConfigurationEvent, ConfigurationEventWrapper>(mapper, out ConfigurationEventWrapper ev))
                {
                    yield return ev;
                }
            }
        }
        public async Task<PulseEventWrapper?> ReceiveLastPulseEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetAliveTopic(key);
            return await TaskGetLastEventFromTopic<PulseEvent, PulseEventWrapper>(topic, cancellationToken);
        }
        public async Task<ConfigurationEventWrapper?> ReceiveLastConfigurationEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetConfigurationTopic(key);
            return await TaskGetLastEventFromTopic<ConfigurationEvent, ConfigurationEventWrapper>(topic, cancellationToken);
        }
        private async Task<Y?> TaskGetLastEventFromTopic<T, Y>(string topic, CancellationToken cancellationToken) where T : BaseEvent where Y : BaseEventWrapper
        {
            ConsumeResponse? response = await messageConsumer.ReadLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);
            if (response != null)
            {
                if (response.TryDeserializeEventWrapper<T, Y>(mapper, out Y ev))
                {
                    return ev;
                }
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