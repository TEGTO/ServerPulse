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
        private readonly IStatisticsDispatcher<T> statisticsDispatcher;
        private readonly ILogger<StatisticsHub<T>> logger;

        public StatisticsHub(IStatisticsDispatcher<T> statisticsDispatcher, ILogger<StatisticsHub<T>> logger)
        {
            this.statisticsDispatcher = statisticsDispatcher;
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

        public async Task StopListen(string key)
        {
            await StopIdListeningKey(key, Context.ConnectionId);
        }

        private async Task StartIdListeningKey(string key, string connectionId)
        {
            await Groups.AddToGroupAsync(connectionId, key);

            await statisticsDispatcher.DispatchInitialStatisticsAsync(key);

            connectedClients.AddOrUpdate(
                connectionId,
                [key],
                (_, keys) =>
                {
                    if (!keys.Contains(key))
                    {
                        keys.Add(key);
                    }
                    return keys;
                }
            );

            listenerAmount.AddOrUpdate(key, 1, (k, count) => count + 1);

            if (listenerAmount.GetValueOrDefault(key) == 1)
            {
                await StartKeyDispatchingAsync(key);
            }
        }

        private async Task StopIdListeningKey(string key, string connectionId)
        {
            await Groups.RemoveFromGroupAsync(connectionId, key);

            listenerAmount.AddOrUpdate(
                key,
                0,
                (k, count) => count <= 1 ? 0 : count - 1
            );

            if (listenerAmount.GetValueOrDefault(key) == 0)
            {
                await StopKeyDispatchingAsync(key);
            }
        }

        private async Task RemoveClientFromGroupAsync(IEnumerable<string> keys, string connectionId)
        {
            await Task.WhenAll(keys.Select(async key =>
            {
                await StopIdListeningKey(key, connectionId);
            }));

            connectedClients.TryRemove(connectionId, out _);
        }

        private async Task StartKeyDispatchingAsync(string dispatchKey)
        {
            var message = $"Start listening to key '{dispatchKey}'";
            logger.LogInformation(message);
            await statisticsDispatcher.StartStatisticsDispatchingAsync(dispatchKey);
        }

        private async Task StopKeyDispatchingAsync(string dispatchKey)
        {
            var message = $"Stop listening to key '{dispatchKey}'";
            logger.LogInformation(message);
            await statisticsDispatcher.StopStatisticsDispatchingAsync(dispatchKey);
        }
    }
}