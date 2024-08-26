using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Hubs;
using AnalyzerApi.Services.Collectors;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace AnalyzerApi.Services
{
    public class StatisticsSender : IStatisticsSender
    {
        #region Fields

        private readonly IHubContext<StatisticsHub<ServerStatisticsCollector>, IStatisticsHubClient> hubStatistics;
        private readonly IHubContext<StatisticsHub<LoadStatisticsCollector>, IStatisticsHubClient> hubLoadStatistics;
        private readonly IHubContext<StatisticsHub<CustomStatisticsCollector>, IStatisticsHubClient> hubCustomEventStatistics;
        private readonly IMessageProducer producer;
        private readonly IMapper mapper;
        private readonly ILogger<StatisticsSender> logger;
        private readonly string serverStatisticsTopic;

        #endregion

        public StatisticsSender(
            IHubContext<StatisticsHub<ServerStatisticsCollector>, IStatisticsHubClient> hubStatistics,
            IHubContext<StatisticsHub<LoadStatisticsCollector>, IStatisticsHubClient> hubLoadStatistics,
            IHubContext<StatisticsHub<CustomStatisticsCollector>, IStatisticsHubClient> hubCustomEventStatistics,
            IMessageProducer producer,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<StatisticsSender> logger)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.producer = producer;
            this.hubStatistics = hubStatistics;
            this.hubLoadStatistics = hubLoadStatistics;
            this.hubCustomEventStatistics = hubCustomEventStatistics;
            serverStatisticsTopic = configuration[Configuration.KAFKA_SERVER_STATISTICS_TOPIC]!;
        }

        #region IStatisticsSender Members

        public async Task SendServerStatisticsAsync(string key, ServerStatistics serverStatistics, CancellationToken cancellationToken)
        {
            var topic = GetTopic(serverStatisticsTopic, key);
            await producer.ProduceAsync(topic, JsonSerializer.Serialize(serverStatistics), cancellationToken);

            var resposnse = mapper.Map<ServerStatisticsResponse>(serverStatistics);
            var serializedData = JsonSerializer.Serialize(resposnse);
            await hubStatistics.Clients.Group(key).ReceiveStatistics(key, serializedData);
        }
        public async Task SendServerLoadStatisticsAsync(string key, ServerLoadStatistics statistics, CancellationToken cancellationToken)
        {
            var response = mapper.Map<ServerLoadStatisticsResponse>(statistics);
            var serializedData = JsonSerializer.Serialize(response);
            await hubLoadStatistics.Clients.Group(key).ReceiveStatistics(key, serializedData);
        }
        public async Task SendServerCustomStatisticsAsync(string key, CustomEventStatistics statistics, CancellationToken cancellationToken)
        {
            var response = mapper.Map<CustomEventStatisticsResponse>(statistics);
            var serializedData = JsonSerializer.Serialize(response);
            await hubCustomEventStatistics.Clients.Group(key).ReceiveStatistics(key, serializedData);
        }

        #endregion

        #region Private Members

        private string GetTopic(string topic, string key)
        {
            return topic + key;
        }

        #endregion
    }
}