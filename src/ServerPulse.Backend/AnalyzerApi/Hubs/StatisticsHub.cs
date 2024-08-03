using AnalyzerApi.Services;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace AnalyzerApi.Hubs
{
    public sealed class StatisticsHub : Hub<IStatisticsHubClient>
    {
        private static readonly ConcurrentDictionary<string, List<string>> ConnectedClients = new();
        private static readonly ConcurrentDictionary<string, int> KeyPulseListeners = new();

        private readonly IServerStatisticsCollector serverStatisticsCollector;
        private readonly ILogger<StatisticsHub> logger;

        public StatisticsHub(IServerStatisticsCollector serverStatisticsCollector, ILogger<StatisticsHub> logger)
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
        public async Task StartListenPulse(string key)
        {
            await AddClientToGroupAsync(key);
            logger.LogInformation($"Start listening pulse with key '{key}'");
            serverStatisticsCollector.StartConsumingStatistics(key);
        }
        private async Task AddClientToGroupAsync(string key)
        {
            KeyPulseListeners.AddOrUpdate(key, 1, (k, count) => count + 1);
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
                if (KeyPulseListeners.TryGetValue(key, out var count))
                {
                    if (count <= 1)
                    {
                        logger.LogInformation($"Stop listening pulse with key '{key}'");
                        KeyPulseListeners.TryRemove(key, out _);
                        serverStatisticsCollector.StopConsumingStatistics(key);
                    }
                    else
                    {
                        KeyPulseListeners[key] = count - 1;
                    }
                }
            }
        }
    }
}