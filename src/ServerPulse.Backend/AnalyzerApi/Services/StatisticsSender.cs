using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Hubs;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace AnalyzerApi.Services
{
    public class StatisticsSender : IStatisticsSender
    {
        private readonly IHubContext<StatisticsHub<ServerStatisticsCollector>, IStatisticsHubClient> hubStatistics;
        private readonly IHubContext<StatisticsHub<LoadStatisticsCollector>, IStatisticsHubClient> hubLoadStatistics;
        private readonly IMessageProducer producer;
        private readonly IMapper mapper;
        private readonly ILogger<StatisticsSender> logger;
        private readonly string serverStatisticsTopic;

        public StatisticsSender(
            IHubContext<StatisticsHub<ServerStatisticsCollector>, IStatisticsHubClient> hubStatistics,
            IHubContext<StatisticsHub<LoadStatisticsCollector>, IStatisticsHubClient> hubLoadStatistics,
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
            serverStatisticsTopic = configuration[Configuration.KAFKA_SERVER_STATISTICS_TOPIC]!;
        }

        public async Task SendServerStatisticsAsync(string key, ServerStatistics serverStatistics, CancellationToken cancellationToken)
        {
            var topic = GetServerStatisticsTopic(key);
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
        private string GetServerStatisticsTopic(string key)
        {
            return serverStatisticsTopic + key;
        }
    }
}