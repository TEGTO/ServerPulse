using AnalyzerApi.Domain.Models;
using AnalyzerApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace AnalyzerApi.Services
{
    public class StatisticsSender : IStatisticsSender
    {
        private readonly IHubContext<StatisticsHub, IStatisticsHubClient> hub;
        private readonly ILogger<StatisticsSender> logger;

        public StatisticsSender(IHubContext<StatisticsHub, IStatisticsHubClient> hub, ILogger<StatisticsSender> logger)
        {
            this.hub = hub;
            this.logger = logger;
        }

        public async Task SendStatisticsAsync(string key, ServerStatistics serverStatistics)
        {
            var serializedData = JsonSerializer.Serialize(serverStatistics);
            logger.LogTrace($"Server with key '{key}' statistics: {serializedData}");
            await hub.Clients.Group(key).ReceiveStatistics(key, serializedData);
        }
    }
}