using AnalyzerApi.Domain.Dtos;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Hubs;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace AnalyzerApi.Services
{
    public class StatisticsSender : IStatisticsSender
    {
        private readonly IHubContext<StatisticsHub<ServerStatisticsCollector>, IStatisticsHubClient> hubStatistics;
        private readonly IHubContext<StatisticsHub<LoadStatisticsCollector>, IStatisticsHubClient> hubLoadStatistics;
        private readonly ILogger<StatisticsSender> logger;
        private readonly IMapper mapper;

        public StatisticsSender(IHubContext<StatisticsHub<ServerStatisticsCollector>, IStatisticsHubClient> serverStatistics,
            IHubContext<StatisticsHub<LoadStatisticsCollector>, IStatisticsHubClient> serverLoadStatistics, ILogger<StatisticsSender> logger, IMapper mapper)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.hubStatistics = serverStatistics;
            this.hubLoadStatistics = serverLoadStatistics;
        }

        public async Task SendServerStatisticsAsync(string key, ServerStatistics serverStatistics)
        {
            var resposnse = mapper.Map<ServerStatisticsResponse>(serverStatistics);
            var serializedData = JsonSerializer.Serialize(resposnse);
            await hubStatistics.Clients.Group(key).ReceiveStatistics(key, serializedData);
        }
        public async Task SendServerLoadStatisticsAsync(string key, ServerLoadStatistics statistics)
        {
            var response = mapper.Map<ServerLoadStatisticsResponse>(statistics);
            var serializedData = JsonSerializer.Serialize(response);
            await hubLoadStatistics.Clients.Group(key).ReceiveStatistics(key, serializedData);
        }
    }
}