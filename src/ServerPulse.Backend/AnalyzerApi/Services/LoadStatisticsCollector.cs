using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using System.Collections.Concurrent;

namespace AnalyzerApi.Services
{
    public class LoadStatisticsCollector : IStatisticsCollector
    {
        private readonly IServerLoadReceiver loadReceiver;
        private readonly IStatisticsSender statisticsSender;
        private readonly ILogger<LoadStatisticsCollector> logger;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> StatisticsListeners = new();

        public LoadStatisticsCollector(IServerLoadReceiver loadReceiver, IStatisticsSender statisticsSender, ILogger<LoadStatisticsCollector> logger)
        {
            this.loadReceiver = loadReceiver;
            this.statisticsSender = statisticsSender;
            this.logger = logger;
        }

        public void StartConsumingStatistics(string key)
        {
            _ = Task.Run(async () =>
            {
                await StartConsumingStatisticsAsync(key);
            });
        }
        private async Task StartConsumingStatisticsAsync(string key)
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                await SendInitialStatisticsAsync(key, cancellationToken);

                if (StatisticsListeners.TryAdd(key, cancellationTokenSource))
                {
                    var tasks = new[]
                    {
                       SubscribeToPulseEventsAsync(key, cancellationToken),
                    };
                    await Task.WhenAll(tasks);
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation($"The operation ConsumeStatisticsAsync with key '{key}' was canceled.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
        public void StopConsumingStatistics(string key)
        {
            if (StatisticsListeners.TryRemove(key, out var tokenSource))
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
            }
        }
        private async Task SendInitialStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            var amountTask = loadReceiver.ReceiveLoadEventAmountByKeyAsync(key, cancellationToken);
            var loadTask = loadReceiver.ReceiveLastLoadEventByKeyAsync(key, cancellationToken);

            await Task.WhenAll(amountTask, loadTask);

            int amountOfEvents = await amountTask;
            var lastLoadEvent = await loadTask;

            var statistics = new ServerLoadStatistics
            {
                AmountOfEvents = amountOfEvents,
                LastEvent = lastLoadEvent,
                CollectedDateUTC = DateTime.UtcNow,
                IsInitial = true
            };
            await statisticsSender.SendServerLoadStatisticsAsync(key, statistics, cancellationToken);
        }
        private async Task SubscribeToPulseEventsAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var load in loadReceiver.ConsumeLoadEventAsync(key, cancellationToken))
            {
                int amountOfEvents = await loadReceiver.ReceiveLoadEventAmountByKeyAsync(key, cancellationToken);
                var statistics = new ServerLoadStatistics
                {
                    AmountOfEvents = amountOfEvents,
                    LastEvent = load,
                    CollectedDateUTC = DateTime.UtcNow
                };
                await statisticsSender.SendServerLoadStatisticsAsync(key, statistics, cancellationToken);
            }
        }
    }
}