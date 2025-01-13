using AnalyzerApi.Application.BackgroundServices;
using AnalyzerApi.Application.Configuration;
using AnalyzerApi.Application.Services.Receivers.Event;
using AnalyzerApi.Application.Services.Receivers.Statistics;
using AnalyzerApi.Application.Services.SerializeStrategies;
using AnalyzerApi.Application.Services.StatisticsDispatchers;
using AnalyzerApi.Application.TopicMapping;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Core.Models.Wrappers;
using EventCommunication.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Shared;

namespace AnalyzerApi.Application
{
    public static class HostApplicationBuilderExtensions
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            #region Options

            var messageBusSettings = builder.Configuration.GetSection(MessageBusSettings.SETTINGS_SECTION).Get<MessageBusSettings>();

            ArgumentNullException.ThrowIfNull(messageBusSettings);

            builder.Services.Configure<MessageBusSettings>(builder.Configuration.GetSection(MessageBusSettings.SETTINGS_SECTION));

            var processSettings = builder.Configuration.GetSection(LoadProcessingSettings.SETTINGS_SECTION).Get<LoadProcessingSettings>();

            ArgumentNullException.ThrowIfNull(processSettings);

            builder.Services.Configure<LoadProcessingSettings>(builder.Configuration.GetSection(LoadProcessingSettings.SETTINGS_SECTION));

            #endregion

            builder.Services.AddAutoMapper(AssemblyReference.Assembly);

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly);
            });

            builder.Services.AddSharedFluentValidation(typeof(AssemblyReference), typeof(ConfigurationEventValidator));

            #region Resilience

            builder.Services.AddResiliencePipeline(processSettings.Resilience, (builder, context) =>
            {
                builder.AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(2),
                    OnRetry = args =>
                    {
                        var logger = context.ServiceProvider.GetService<ILogger<ResiliencePipelineBuilder>>();
                        var message = $"Retrying after failure. Attempt {args.AttemptNumber}";
                        logger?.LogWarning(args.Outcome.Exception, message);
                        return ValueTask.CompletedTask;
                    }
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5, // Open the circuit if 50% of attempts fail
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromSeconds(15),
                    OnOpened = args =>
                    {
                        var logger = context.ServiceProvider.GetService<ILogger<ResiliencePipelineBuilder>>();
                        logger?.LogWarning("Circuit breaker opened due to repeated failures.");
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = args =>
                    {
                        var logger = context.ServiceProvider.GetService<ILogger<ResiliencePipelineBuilder>>();
                        logger?.LogInformation("Circuit breaker closed. Resuming normal operations.");
                        return ValueTask.CompletedTask;
                    }
                });
            });

            #endregion

            #region Project Services

            #region Event Receiver

            builder.Services.AddSingleton(new EventTopicMapping<ConfigurationEventWrapper>(messageBusSettings.ConfigurationTopic));
            builder.Services.AddSingleton(new EventTopicMapping<CustomEventWrapper>(messageBusSettings.CustomTopic));
            builder.Services.AddSingleton(new EventTopicMapping<LoadEventWrapper>(messageBusSettings.LoadTopic));
            builder.Services.AddSingleton(new EventTopicMapping<PulseEventWrapper>(messageBusSettings.AliveTopic));

            builder.Services.AddSingleton<IEventSerializeStrategy<ConfigurationEventWrapper>, ConfigurationEventSerializeStrategy>();
            builder.Services.AddSingleton<IEventSerializeStrategy<CustomEventWrapper>, CustomEventSerializeStrategy>();
            builder.Services.AddSingleton<IEventSerializeStrategy<LoadEventWrapper>, LoadEventSerializeStrategy>();
            builder.Services.AddSingleton<IEventSerializeStrategy<PulseEventWrapper>, PulseEventSerializeStrategy>();

            builder.Services.AddSingleton<IEventReceiver<ConfigurationEventWrapper>, EventReceiver<ConfigurationEventWrapper>>();
            builder.Services.AddSingleton<IEventReceiver<CustomEventWrapper>, EventReceiver<CustomEventWrapper>>();
            builder.Services.AddSingleton<IEventReceiver<LoadEventWrapper>, EventReceiver<LoadEventWrapper>>();
            builder.Services.AddSingleton<IEventReceiver<PulseEventWrapper>, EventReceiver<PulseEventWrapper>>();

            #endregion

            #region Statistics Receiver

            builder.Services.AddSingleton(new StatisticsTopicMapping<ServerCustomStatistics>(messageBusSettings.CustomTopic));
            builder.Services.AddSingleton(new StatisticsTopicMapping<LoadAmountStatistics>(messageBusSettings.LoadTopic));
            builder.Services.AddSingleton(new StatisticsTopicMapping<LoadMethodStatistics>(messageBusSettings.LoadMethodStatisticsTopic));
            builder.Services.AddSingleton(new StatisticsTopicMapping<ServerLoadStatistics>(messageBusSettings.LoadTopic));
            builder.Services.AddSingleton(new StatisticsTopicMapping<ServerLifecycleStatistics>(messageBusSettings.ServerStatisticsTopic));

            builder.Services.AddSingleton<IStatisticsReceiver<ServerCustomStatistics>, StatisticsReceiver<ServerCustomStatistics>>();
            builder.Services.AddSingleton<IStatisticsReceiver<LoadMethodStatistics>, StatisticsReceiver<LoadMethodStatistics>>();
            builder.Services.AddSingleton<IStatisticsReceiver<ServerLoadStatistics>, StatisticsReceiver<ServerLoadStatistics>>();
            builder.Services.AddSingleton<IStatisticsReceiver<ServerLifecycleStatistics>, StatisticsReceiver<ServerLifecycleStatistics>>();
            builder.Services.AddSingleton<LoadAmountStatisticsReceiver>();
            builder.Services.AddSingleton<IStatisticsReceiver<LoadAmountStatistics>>(provider => provider.GetRequiredService<LoadAmountStatisticsReceiver>());
            builder.Services.AddSingleton<ILoadAmountStatisticsReceiver>(provider => provider.GetRequiredService<LoadAmountStatisticsReceiver>());

            #endregion

            builder.Services.AddSingleton<IStatisticsDispatcher<ServerLifecycleStatistics>, LifecycleStatisticsDispatcher>();
            builder.Services.AddSingleton<IStatisticsDispatcher<ServerLoadStatistics>, StatisticsDispatcher<ServerLoadStatistics, LoadEventWrapper>>();
            builder.Services.AddSingleton<IStatisticsDispatcher<ServerCustomStatistics>, StatisticsDispatcher<ServerCustomStatistics, CustomEventWrapper>>();

            #endregion

            builder.Services.AddHostedService<LoadEventStatisticsProcessor>();

            return builder;
        }
    }
}
