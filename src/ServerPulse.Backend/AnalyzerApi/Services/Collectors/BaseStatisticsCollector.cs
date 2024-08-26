using AnalyzerApi.Services.Interfaces;
using System.Collections.Concurrent;

namespace AnalyzerApi.Services.Collectors
{
    public abstract class BaseStatisticsCollector
    {
        protected readonly IStatisticsSender statisticsSender;
        protected readonly ILogger logger;
        protected readonly ConcurrentDictionary<string, CancellationTokenSource> statisticsListeners = new();

        protected BaseStatisticsCollector(IStatisticsSender statisticsSender, ILogger logger)
        {
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

        protected async Task StartConsumingStatisticsAsync(string key)
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                await SendInitialStatisticsAsync(key, cancellationToken);

                if (statisticsListeners.TryAdd(key, cancellationTokenSource))
                {
                    var tasks = GetEventSubscriptionTasks(key, cancellationToken);
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
            if (statisticsListeners.TryRemove(key, out var tokenSource))
            {
                OnStatisticsListenerRemoved(key);
                tokenSource.Cancel();
                tokenSource.Dispose();
            }
        }

        protected abstract Task SendInitialStatisticsAsync(string key, CancellationToken cancellationToken);

        protected abstract Task[] GetEventSubscriptionTasks(string key, CancellationToken cancellationToken);

        protected virtual void OnStatisticsListenerRemoved(string key) { }
    }
}