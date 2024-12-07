using AnalyzerApi.Hubs;
using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace AnalyzerApi.Services
{
    public class StatisticsSender : IStatisticsSender
    {
        private readonly IHubContext<StatisticsHub<ServerStatistics>, IStatisticsHubClient> hubStatistics;
        private readonly IHubContext<StatisticsHub<ServerLoadStatistics>, IStatisticsHubClient> hubLoadStatistics;
        private readonly IHubContext<StatisticsHub<ServerCustomStatistics>, IStatisticsHubClient> hubCustomEventStatistics;
        private readonly IMessageProducer producer;
        private readonly IMapper mapper;
        private readonly string serverStatisticsTopic;

        public StatisticsSender(
            IHubContext<StatisticsHub<ServerStatistics>, IStatisticsHubClient> hubStatistics,
            IHubContext<StatisticsHub<ServerLoadStatistics>, IStatisticsHubClient> hubLoadStatistics,
            IHubContext<StatisticsHub<ServerCustomStatistics>, IStatisticsHubClient> hubCustomEventStatistics,
            IMessageProducer producer,
            IMapper mapper,
            IConfiguration configuration)
        {
            this.mapper = mapper;
            this.producer = producer;
            this.hubStatistics = hubStatistics;
            this.hubLoadStatistics = hubLoadStatistics;
            this.hubCustomEventStatistics = hubCustomEventStatistics;
            serverStatisticsTopic = configuration[Configuration.KAFKA_SERVER_STATISTICS_TOPIC]!;
        }

        #region IStatisticsSender Members

        public Task SendStatisticsAsync<TStatistics>(string key, TStatistics statistics, CancellationToken cancellationToken) where TStatistics : BaseStatistics
        {
            switch (statistics)
            {
                case ServerStatistics:
                    return SendServerStatisticsAsync(key, statistics as ServerStatistics, cancellationToken);
                case ServerLoadStatistics:
                    return SendServerLoadStatisticsAsync(key, statistics as ServerLoadStatistics);
                case ServerCustomStatistics:
                    return SendServerCustomStatisticsAsync(key, statistics as ServerCustomStatistics);
                default:
                    return Task.CompletedTask;
            }
        }

        #endregion

        #region Private Helpers

        private async Task SendServerStatisticsAsync(string key, ServerStatistics serverStatistics, CancellationToken cancellationToken)
        {
            var topic = serverStatisticsTopic + key;
            await producer.ProduceAsync(topic, JsonSerializer.Serialize(serverStatistics), cancellationToken);

            var response = mapper.Map<ServerStatisticsResponse>(serverStatistics);
            var serializedData = JsonSerializer.Serialize(response);
            await hubStatistics.Clients.Group(key).ReceiveStatistics(key, serializedData);
        }
        private async Task SendServerLoadStatisticsAsync(string key, ServerLoadStatistics statistics)
        {
            var response = mapper.Map<ServerLoadStatisticsResponse>(statistics);
            var serializedData = JsonSerializer.Serialize(response);
            await hubLoadStatistics.Clients.Group(key).ReceiveStatistics(key, serializedData);
        }
        private async Task SendServerCustomStatisticsAsync(string key, ServerCustomStatistics statistics)
        {
            var response = mapper.Map<CustomEventStatisticsResponse>(statistics);
            var serializedData = JsonSerializer.Serialize(response);
            await hubCustomEventStatistics.Clients.Group(key).ReceiveStatistics(key, serializedData);
        }

        #endregion
    }
}