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
        private readonly ConcurrentDictionary<string, SemaphoreSlim> keyLocks = new();
        private readonly IStatisticsDispatcher<T> statisticsDispatcher;
        private readonly ILogger<StatisticsHub<T>> logger;

        public StatisticsHub(IStatisticsDispatcher<T> serverStatisticsCollector, ILogger<StatisticsHub<T>> logger)
        {
            this.statisticsDispatcher = serverStatisticsCollector;
            this.logger = logger;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (connectedClients.TryGetValue(Context.ConnectionId, out var keys))
            {
                await RemoveClientFromGroupAsync(keys, Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task StartListen(string key)
        {
            await StartIdListeningKey(key, Context.ConnectionId);
        }

        private async Task StartIdListeningKey(string key, string connectionId)
        {
            connectedClients.AddOrUpdate(
                connectionId,
                [key],
                (_, keys) =>
                {
                    if (!keys.Contains(key)) keys.Add(key);
                    return keys;
                }
            );

            await Groups.AddToGroupAsync(connectionId, key);

            await statisticsDispatcher.DispatchInitialStatistics(key);

            var keyLock = keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await keyLock.WaitAsync();
            try
            {
                var isKeyBeingListening = listenerAmount.ContainsKey(key) && listenerAmount[key] > 0;

                if (!isKeyBeingListening)
                {
                    string message = $"Start listening to key '{key}'";
                    logger.LogInformation(message);

                    statisticsDispatcher.StartStatisticsDispatching(key);
                }

                listenerAmount.AddOrUpdate(key, 1, (k, count) => count + 1);
            }
            finally
            {
                keyLock.Release();
            }
        }

        private async Task AddClientToGroupAsync(string key, string connectionId)
        {
            listenerAmount.AddOrUpdate(key, 1, (k, count) => count + 1);

            connectedClients.AddOrUpdate(
                connectionId,
                [key],
                (_, keys) =>
                {
                    if (!keys.Contains(key)) keys.Add(key);
                    return keys;
                }
            );

            await Groups.AddToGroupAsync(connectionId, key);
        }

        private async Task RemoveClientFromGroupAsync(IEnumerable<string> keys, string connectionId)
        {
            foreach (var key in keys)
            {
                await Groups.RemoveFromGroupAsync(connectionId, key);

                listenerAmount.AddOrUpdate(
                   key,
                   0,
                   (k, count) =>
                   {
                       if (count <= 1)
                       {
                           string message = $"Stop listening to key '{k}'";
                           logger.LogInformation(message);

                           statisticsDispatcher.StopStatisticsDispatching(k);

                           return 0;
                       }
                       return count - 1;
                   }
                );
            }

            connectedClients.TryRemove(connectionId, out _);
        }
    }
}