using AnalyzerApi.Command.Builders;
using AnalyzerApi.Command.Senders;
using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using MediatR;
using System.Collections.Concurrent;

namespace AnalyzerApi.Services.StatisticsDispatchers
{
    public sealed class LifecycleStatisticsDispatcher : StatisticsDispatcher<ServerLifecycleStatistics, PulseEventWrapper>
    {
        private readonly IEventReceiver<ConfigurationEventWrapper> confReceiver;
        private readonly ConcurrentDictionary<string, (PeriodicTimer Timer, int ServerUpdateInterval, bool IsAlive)> listenerState = new();
        private readonly int sendPeriodInMilliseconds;

        public LifecycleStatisticsDispatcher(
            IEventReceiver<PulseEventWrapper> receiver,
            IEventReceiver<ConfigurationEventWrapper> confReceiver,
            IMediator mediator,
            IConfiguration configuration,
            ILogger<LifecycleStatisticsDispatcher> logger)
            : base(receiver, mediator, logger)
        {
            this.confReceiver = confReceiver;
            sendPeriodInMilliseconds = int.Parse(configuration[Configuration.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS]!);
        }

        protected override Task[] DispatchingTasks(string key, CancellationToken cancellationToken)
        {
            return
            [
                MonitorPulseEventsAsync(key, cancellationToken),
                SendStatisticsAsync(key, cancellationToken)
            ];
        }

        protected override void OnListenerRemoved(string key)
        {
            if (listenerState.TryRemove(key, out var state))
            {
                state.Timer.Dispose();
            }
        }

        #region Private Helpers

        private async Task SendStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            listenerState[key] = (new PeriodicTimer(TimeSpan.FromMilliseconds(sendPeriodInMilliseconds)), 1000, true);

            while (await listenerState[key].Timer.WaitForNextTickAsync(cancellationToken))
            {
                var (timer, updateInterval, isAlive) = listenerState[key];

                if (isAlive)
                {
                    var statistics = await mediator.Send(new BuildStatisticsCommand<ServerLifecycleStatistics>(key), cancellationToken);
                    listenerState[key] = (timer, updateInterval, statistics.IsAlive);
                    await mediator.Send(new SendStatisticsCommand<ServerLifecycleStatistics>(key, statistics), cancellationToken);
                }

                var conf = await confReceiver.GetLastEventByKeyAsync(key, cancellationToken);
                if (conf != null)
                {
                    listenerState[key] = (timer, (int)conf.ServerKeepAliveInterval.TotalMilliseconds, isAlive);
                }

                await Task.Delay(listenerState[key].ServerUpdateInterval, cancellationToken);
            }
        }

        private async Task MonitorPulseEventsAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var pulse in receiver.GetEventStreamAsync(key, cancellationToken))
            {
                if (listenerState.TryGetValue(key, out var state))
                {
                    listenerState[key] = (state.Timer, state.ServerUpdateInterval, pulse.IsAlive);
                }
            }
        }

        #endregion
    }
}