using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using MessageBus.Interfaces;
using ServerPulse.EventCommunication.Events;
using Shared;

namespace AnalyzerApi.Services
{
    public class ServerStatusReceiver : BaseEventReceiver, IServerStatusReceiver
    {
        #region Fields

        private readonly string aliveTopic;
        private readonly string configurationTopic;
        private readonly string serverStatisticsTopic;

        #endregion

        public ServerStatusReceiver(IMessageConsumer messageConsumer, IMapper mapper, IConfiguration configuration)
            : base(messageConsumer, mapper, configuration)
        {
            aliveTopic = configuration[Configuration.KAFKA_ALIVE_TOPIC]!;
            configurationTopic = configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]!;
            serverStatisticsTopic = configuration[Configuration.KAFKA_SERVER_STATISTICS_TOPIC]!;
        }

        #region IServerStatusReceiver Members

        public IAsyncEnumerable<PulseEventWrapper> ConsumePulseEventAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(aliveTopic, key);
            return ConsumeEventAsync<PulseEvent, PulseEventWrapper>(topic, cancellationToken);
        }

        public IAsyncEnumerable<ConfigurationEventWrapper> ConsumeConfigurationEventAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(configurationTopic, key);
            return ConsumeEventAsync<ConfigurationEvent, ConfigurationEventWrapper>(topic, cancellationToken);
        }

        public Task<ServerStatistics?> ReceiveLastServerStatisticsByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(serverStatisticsTopic, key);
            return ReceiveLastServerStatisticsAsync(topic, cancellationToken);
        }

        public Task<PulseEventWrapper?> ReceiveLastPulseEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(aliveTopic, key);
            return ReceiveLastEventByKeyAsync<PulseEvent, PulseEventWrapper>(topic, cancellationToken);
        }

        public Task<ConfigurationEventWrapper?> ReceiveLastConfigurationEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(configurationTopic, key);
            return ReceiveLastEventByKeyAsync<ConfigurationEvent, ConfigurationEventWrapper>(topic, cancellationToken);
        }

        #endregion

        #region Private Helpers

        private async Task<ServerStatistics?> ReceiveLastServerStatisticsAsync(string topic, CancellationToken cancellationToken)
        {
            ConsumeResponse? response = await messageConsumer.ReadLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);
            if (response != null && response.Message.TryToDeserialize(out ServerStatistics stat))
            {
                return stat;
            }
            return null;
        }

        #endregion
    }
}