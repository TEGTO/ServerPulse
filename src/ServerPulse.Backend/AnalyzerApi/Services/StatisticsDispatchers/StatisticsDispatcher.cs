using AnalyzerApi.Command.Builders;
using AnalyzerApi.Command.Senders;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using MediatR;
using System.Collections.Concurrent;

namespace AnalyzerApi.Services.StatisticsDispatchers
{
    public class StatisticsDispatcher<TStatistics, TEventWrapper> : IStatisticsDispatcher<TStatistics>, IAsyncDisposable
        where TStatistics : BaseStatistics
        where TEventWrapper : BaseEventWrapper
    {
        protected readonly IEventReceiver<TEventWrapper> receiver;
        protected readonly IMediator mediator;
        protected readonly ILogger<StatisticsDispatcher<TStatistics, TEventWrapper>> logger;
        protected readonly ConcurrentDictionary<string, CancellationTokenSource> listeners = new();

        public StatisticsDispatcher(
            IEventReceiver<TEventWrapper> receiver,
            IMediator mediator,
            ILogger<StatisticsDispatcher<TStatistics, TEventWrapper>> logger)
        {
            this.receiver = receiver;
            this.mediator = mediator;
            this.logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            var tasks = listeners.Keys.Select(StopStatisticsDispatchingAsync).ToArray();
            await Task.WhenAll(tasks);
        }

        public async Task DispatchInitialStatisticsAsync(string key, CancellationToken cancellationToken = default)
        {
            var statistics = await mediator.Send(new BuildStatisticsCommand<TStatistics>(key), cancellationToken);
            if (statistics != null)
            {
                await mediator.Send(new SendStatisticsCommand<TStatistics>(key, statistics), cancellationToken);
            }
        }

        public Task StartStatisticsDispatchingAsync(string key)
        {
            if (listeners.ContainsKey(key))
            {
                return Task.CompletedTask;
            }

            var cancellationTokenSource = new CancellationTokenSource();

            if (listeners.TryAdd(key, cancellationTokenSource))
            {
                Task.Factory.StartNew(
                    () => DispatchStatisticsAsync(key, cancellationTokenSource.Token),
                    cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default
                );
            }
            return Task.CompletedTask;
        }

        public async Task StopStatisticsDispatchingAsync(string key)
        {
            if (listeners.TryRemove(key, out var tokenSource))
            {
                try
                {
                    await tokenSource.CancelAsync();
                    tokenSource.Dispose();
                    OnListenerRemoved(key);
                }
                catch (Exception ex)
                {
                    var message = $"Failed to stop dispatching for key '{key}'.";
                    logger.LogError(ex, message);
                }
            }
        }

        private async Task DispatchStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            try
            {
                var message = $"Started dispatching for key '{key}'.";
                logger.LogInformation(message);

                var tasks = DispatchingTasks(key, cancellationToken);
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException ex)
            {
                var message = $"Dispatching for key '{key}' was canceled.";
                logger.LogInformation(ex, message);
            }
            catch (Exception ex)
            {
                var message = $"Error occurred while dispatching for key '{key}'.";
                logger.LogError(ex, message);
            }
        }

        protected virtual Task[] DispatchingTasks(string key, CancellationToken cancellationToken)
        {
            return
            [
                SendNewStatisticsOnEveryUpdateAsync(key, cancellationToken)
            ];
        }

        protected virtual void OnListenerRemoved(string key)
        {
            var message = $"Stopped listening for key '{key}'.";
            logger.LogInformation(message);
        }

        private async Task SendNewStatisticsOnEveryUpdateAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var ev in receiver.GetEventStreamAsync(key, cancellationToken))
            {
                var statistics = await mediator.Send(new BuildStatisticsCommand<TStatistics>(key), cancellationToken);
                await mediator.Send(new SendStatisticsCommand<TStatistics>(key, statistics), cancellationToken);
            }
        }
    }
}