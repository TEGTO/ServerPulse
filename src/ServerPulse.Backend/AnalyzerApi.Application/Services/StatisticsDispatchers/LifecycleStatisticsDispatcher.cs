using AnalyzerApi.Application.Command.Builders;
using AnalyzerApi.Application.Command.Senders;
using AnalyzerApi.Application.Services.Receivers.Event;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Core.Models.Wrappers;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AnalyzerApi.Application.Services.StatisticsDispatchers
{
    public sealed class LifecycleStatisticsDispatcher : StatisticsDispatcher<ServerLifecycleStatistics, PulseEventWrapper>
    {
        internal sealed record ListenerState(PeriodicTimer Timer, bool IsAlive);

        private readonly IEventReceiver<ConfigurationEventWrapper> cnfReceiver;
        private readonly ConcurrentDictionary<string, ListenerState> listenerState = new();
        private readonly int minimalSendPeriodInMilliseconds;

        public LifecycleStatisticsDispatcher(
            IEventReceiver<PulseEventWrapper> receiver,
            IEventReceiver<ConfigurationEventWrapper> cnfReceiver,
            IMediator mediator,
            IConfiguration configuration,
            ILogger<LifecycleStatisticsDispatcher> logger)
            : base(receiver, mediator, logger)
        {
            this.cnfReceiver = cnfReceiver;
            minimalSendPeriodInMilliseconds = int.Parse(configuration[ConfigurationKeys.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS]!);
        }

        protected override Task[] DispatchingTasks(string key, CancellationToken cancellationToken)
        {
            return
            [
                SendStatisticsPeriodicallyAsync(key, cancellationToken),
                MonitorPulseEventAsync(key, cancellationToken),
                MonitorConfigurationEventAsync(key, cancellationToken)
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

        private async Task SendStatisticsPeriodicallyAsync(string key, CancellationToken cancellationToken)
        {
            await ConfigureListenerStateAsync(key, cancellationToken);

            while (await listenerState[key].Timer.WaitForNextTickAsync(cancellationToken))
            {
                await SendStatisticsAsync(key, cancellationToken);
            }
        }

        private async Task ConfigureListenerStateAsync(string key, CancellationToken cancellationToken)
        {
            listenerState[key] = new ListenerState(new PeriodicTimer(TimeSpan.FromMilliseconds(minimalSendPeriodInMilliseconds)), true);

            var configuration = await cnfReceiver.GetLastEventByKeyAsync(key, cancellationToken);
            HandleConfigurationEvent(key, configuration);
        }

        private async Task MonitorPulseEventAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var pulse in receiver.GetEventStreamAsync(key, cancellationToken))
            {
                await HandlePulseEventAsync(key, pulse, cancellationToken);
            }
        }

        private async Task MonitorConfigurationEventAsync(string key, CancellationToken cancellationToken)
        {
            await foreach (var configuration in cnfReceiver.GetEventStreamAsync(key, cancellationToken))
            {
                HandleConfigurationEvent(key, configuration);
            }
        }

        private async Task SendStatisticsAsync(string key, CancellationToken cancellationToken)
        {
            if (!listenerState.TryGetValue(key, out var state)) return;

            var statistics = await mediator.Send(new BuildStatisticsCommand<ServerLifecycleStatistics>(key), cancellationToken);
            listenerState[key] = state with { IsAlive = statistics.IsAlive };

            await mediator.Send(new SendStatisticsCommand<ServerLifecycleStatistics>(key, statistics), cancellationToken);
        }

        private void HandleConfigurationEvent(string key, ConfigurationEventWrapper? configuration)
        {
            if (configuration == null || !listenerState.TryGetValue(key, out var state)) return;

            var newPeriod = Math.Max(minimalSendPeriodInMilliseconds, (int)configuration.ServerKeepAliveInterval.TotalMilliseconds / 2);
            state.Timer.Period = TimeSpan.FromMilliseconds(newPeriod);
        }

        private async Task HandlePulseEventAsync(string key, PulseEventWrapper? pulse, CancellationToken cancellationToken)
        {
            if (listenerState.TryGetValue(key, out var state) && pulse != null && state.IsAlive != pulse.IsAlive)
            {
                await SendStatisticsAsync(key, cancellationToken);
            }
        }

        #endregion
    }
}