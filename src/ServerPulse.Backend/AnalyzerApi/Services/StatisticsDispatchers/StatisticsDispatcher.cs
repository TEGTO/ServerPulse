using AnalyzerApi.Command.Builders;
using AnalyzerApi.Command.Senders;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using MediatR;
using System.Collections.Concurrent;

namespace AnalyzerApi.Services.StatisticsDispatchers
{
    public class StatisticsDispatcher<TStatistics, TEventWrapper> : IStatisticsDispatcher<TStatistics>
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

        public void StartStatisticsDispatching(string key)
        {
            Task.Run(() => DispatchStatisticsAsync(key));
        }

        public void StopStatisticsDispatching(string key)
        {
            if (listeners.TryRemove(key, out var tokenSource))
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
                OnListenerRemoved(key);
            }
        }

        private async Task DispatchStatisticsAsync(string key)
        {
            try
            {
                using var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                if (!listeners.TryAdd(key, cancellationTokenSource))
                    return;

                await SendInitialStatisticsAsync(key, cancellationToken);

                await Task.WhenAll(GetEventSubscriptionTasks(key, cancellationToken));
            }
            catch (OperationCanceledException ex)
            {
                string message = $"Dispatching for key '{key}' was canceled.";
                logger.LogInformation(ex, message);
            }
            catch (Exception ex)
            {
                string message = $"Error occurred while dispatching for key '{key}'.";
                logger.LogError(ex, message);
            }
        }

        protected virtual async Task SendInitialStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            var statistics = await mediator.Send(new BuildStatisticsCommand<TStatistics>(key), cancellationToken);
            if (statistics != null)
            {
                statistics.IsInitial = true;
                await mediator.Send(new SendStatisticsCommand<TStatistics>(key, statistics), cancellationToken);
            }
        }

        protected virtual Task[] GetEventSubscriptionTasks(string key, CancellationToken cancellationToken)
        {
            return
            [
                SendNewStatisticsOnEveryUpdateAsync(key, cancellationToken)
            ];
        }

        protected virtual void OnListenerRemoved(string key)
        {
            string message = $"Stopped listening for key '{key}'.";
            logger.LogInformation(message);
        }

        private async Task SendNewStatisticsOnEveryUpdateAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var load in receiver.GetEventStreamAsync(key, cancellationToken))
            {
                var statistics = await mediator.Send(new BuildStatisticsCommand<TStatistics>(key), cancellationToken);
                await mediator.Send(new SendStatisticsCommand<TStatistics>(key, statistics), cancellationToken);
            }
        }

    }

}