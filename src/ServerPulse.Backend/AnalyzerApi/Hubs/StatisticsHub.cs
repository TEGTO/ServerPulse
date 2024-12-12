using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Services.StatisticsDispatchers;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace AnalyzerApi.Hubs
{
    public sealed class StatisticsHub<T> : Hub<IStatisticsHubClient> where T : BaseStatistics
    {
        private readonly ConcurrentDictionary<string, List<string>> connectedClients = new();
        private readonly ConcurrentDictionary<string, int> listenerAmount = new();
        private readonly IStatisticsDispatcher<T> serverStatisticsCollector;
        private readonly ILogger<StatisticsHub<T>> logger;

        public StatisticsHub(IStatisticsDispatcher<T> serverStatisticsCollector, ILogger<StatisticsHub<T>> logger)
        {
            this.serverStatisticsCollector = serverStatisticsCollector;
            this.logger = logger;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (connectedClients.TryGetValue(Context.ConnectionId, out var keys))
            {
                await RemoveClientFromGroupAsync(keys);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task StartListen(string key)
        {
            await AddClientToGroupAsync(key);

            string message = $"Start listening to key '{key}'";
            logger.LogInformation(message);

            serverStatisticsCollector.StartStatisticsDispatching(key);
        }

        private async Task AddClientToGroupAsync(string key)
        {
            listenerAmount.AddOrUpdate(key, 1, (k, count) => count + 1);

            connectedClients.AddOrUpdate(
                Context.ConnectionId,
                [key],
                (_, keys) =>
                {
                    if (!keys.Contains(key)) keys.Add(key);
                    return keys;
                }
            );

            await Groups.AddToGroupAsync(Context.ConnectionId, key);
        }

        private async Task RemoveClientFromGroupAsync(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, key);

                listenerAmount.AddOrUpdate(
                   key,
                   0,
                   (k, count) =>
                   {
                       if (count <= 1)
                       {
                           string message = $"Stop listening to key '{k}'";
                           logger.LogInformation(message);

                           serverStatisticsCollector.StopStatisticsDispatching(k);

                           return 0;
                       }
                       return count - 1;
                   }
                );
            }
        }
    }
}