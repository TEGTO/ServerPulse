using AnalyzerApi.Domain.Dtos;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Hubs;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace AnalyzerApi.Services
{
    public class StatisticsSender : IStatisticsSender
    {
        private readonly IHubContext<StatisticsHub, IStatisticsHubClient> hub;
        private readonly ILogger<StatisticsSender> logger;
        private readonly IMapper mapper;

        public StatisticsSender(IHubContext<StatisticsHub, IStatisticsHubClient> hub, ILogger<StatisticsSender> logger, IMapper mapper)
        {
            this.hub = hub;
            this.logger = logger;
            this.mapper = mapper;
        }

        public async Task SendStatisticsAsync(string key, ServerStatistics serverStatistics)
        {
            var resposnse = mapper.Map<ServerStatisticsResponse>(serverStatistics);
            var serializedData = JsonSerializer.Serialize(resposnse);
            logger.LogTrace($"Server with key '{key}' statistics: {serializedData}");
            await hub.Clients.Group(key).ReceiveStatistics(key, serializedData);
        }
    }
}