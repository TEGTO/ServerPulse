using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using System.Collections.Concurrent;

namespace AnalyzerApi.Services.Consumers
{
    public class StatisticsConsumer<TStatistics, TEventWrapper> : IStatisticsConsumer<TStatistics>
        where TStatistics : BaseStatistics
        where TEventWrapper : BaseEventWrapper
    {
        #region Fields

        protected readonly IStatisticsCollector<TStatistics> collector;
        protected readonly IEventReceiver<TEventWrapper> receiver;
        protected readonly IStatisticsSender statisticsSender;
        protected readonly ILogger<StatisticsConsumer<TStatistics, TEventWrapper>> logger;
        protected readonly ConcurrentDictionary<string, CancellationTokenSource> statisticsListeners = new();

        #endregion

        public StatisticsConsumer(
            IStatisticsCollector<TStatistics> collector,
            IEventReceiver<TEventWrapper> receiver,
            IStatisticsSender statisticsSender,
            ILogger<StatisticsConsumer<TStatistics, TEventWrapper>> logger)
        {
            this.collector = collector;
            this.receiver = receiver;
            this.statisticsSender = statisticsSender;
            this.logger = logger;
        }

        #region IStatisticsConsumer Members

        public void StartConsumingStatistics(string key)
        {
            _ = Task.Run(async () =>
            {
                await StartConsumingStatisticsAsync(key);
            });
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

        #endregion

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
        protected virtual async Task SendInitialStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            var statistics = await collector.ReceiveLastStatisticsAsync(key, cancellationToken);
            if (statistics != null)
            {
                statistics.IsInitial = true;
                await statisticsSender.SendStatisticsAsync(key, statistics, cancellationToken);
            }
        }
        protected virtual Task[] GetEventSubscriptionTasks(string key, CancellationToken cancellationToken)
        {
            return new[]
            {
                SubscribeToEventAsync(key, cancellationToken)
            };
        }
        protected virtual void OnStatisticsListenerRemoved(string key) { }

        private async Task SubscribeToEventAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var load in receiver.ConsumeEventAsync(key, cancellationToken))
            {
                var statistics = await collector.ReceiveLastStatisticsAsync(key, cancellationToken);
                await statisticsSender.SendStatisticsAsync(key, statistics, cancellationToken);
            }
        }
    }
}