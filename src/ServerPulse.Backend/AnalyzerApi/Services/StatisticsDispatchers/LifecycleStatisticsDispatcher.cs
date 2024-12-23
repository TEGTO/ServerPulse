﻿using AnalyzerApi.Command.Builders;
using AnalyzerApi.Command.Senders;
using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using MediatR;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnalyzerApiTests")]
namespace AnalyzerApi.Services.StatisticsDispatchers
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
            minimalSendPeriodInMilliseconds = int.Parse(configuration[Configuration.STATISTICS_COLLECT_INTERVAL_IN_MILLISECONDS]!);
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