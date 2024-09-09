using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace AnalyzerApi.Hubs
{
    public sealed class StatisticsHub<T> : Hub<IStatisticsHubClient> where T : BaseStatistics
    {
        private static readonly ConcurrentDictionary<string, List<string>> ConnectedClients = new();
        private static readonly ConcurrentDictionary<string, int> ListenerAmount = new();

        private readonly IStatisticsConsumer<T> serverStatisticsCollector;
        private readonly ILogger<StatisticsHub<T>> logger;

        public StatisticsHub(IStatisticsConsumer<T> serverStatisticsCollector, ILogger<StatisticsHub<T>> logger)
        {
            this.serverStatisticsCollector = serverStatisticsCollector;
            this.logger = logger;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectedClients.TryGetValue(Context.ConnectionId, out var keys))
            {
                await RemoveClientFromGroupAsync(keys);
            }
        }
        public async Task StartListen(string key)
        {
            await AddClientToGroupAsync(key);
            logger.LogInformation($"Start listening key '{key}'");
            serverStatisticsCollector.StartConsumingStatistics(key);
        }
        private async Task AddClientToGroupAsync(string key)
        {
            ListenerAmount.AddOrUpdate(key, 1, (k, count) => count + 1);
            ConnectedClients.AddOrUpdate(Context.ConnectionId, new List<string>() { key },
            (k, keys) =>
            {
                keys.Add(key);
                return keys;
            });
            await Groups.AddToGroupAsync(Context.ConnectionId, key);
        }
        private async Task RemoveClientFromGroupAsync(List<string> keys)
        {
            foreach (var key in keys)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, key);
                if (ListenerAmount.TryGetValue(key, out var count))
                {
                    if (count <= 1)
                    {
                        logger.LogInformation($"Stop listening key '{key}'");
                        ListenerAmount.TryRemove(key, out _);
                        serverStatisticsCollector.StopConsumingStatistics(key);
                    }
                    else
                    {
                        ListenerAmount[key] = count - 1;
                    }
                }
            }
        }
    }
}