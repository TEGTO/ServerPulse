using AnalyzerApi.Application.Application.Services;
using AnalyzerApi.Application.BackgroundServices;
using AnalyzerApi.Application.Configuration;
using AnalyzerApi.Application.Services.Receivers.Event;
using AnalyzerApi.Application.Services.Receivers.Statistics;
using AnalyzerApi.Application.Services.SerializeStrategies;
using AnalyzerApi.Application.Services.StatisticsDispatchers;
using AnalyzerApi.Application.TopicMapping;
using AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetLoadAmountStatisticsInRange;
using AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetLoadEventsInDataRange;
using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Core.Models.Wrappers;
using AnalyzerApi.Infrastructure.Services;
using AnalyzerApi.Infrastructure.Settings;
using Caching;
using Confluent.Kafka;
using EventCommunication.Validators;
using MessageBus;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Shared;
using ApplicationAssembly = AnalyzerApi.Application.AssemblyReference;

namespace AnalyzerApi
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

            builder.Services.AddAutoMapper(ApplicationAssembly.Assembly);

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(ApplicationAssembly.Assembly);
            });

            builder.Services.AddSharedFluentValidation(typeof(ApplicationAssembly), typeof(ConfigurationEventValidator));

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

        public static IHostApplicationBuilder AddInfrastructureServices(this IHostApplicationBuilder builder)
        {
            #region Options

            var kafkaSettings = builder.Configuration.GetSection(KafkaSettings.SETTINGS_SECTION).Get<KafkaSettings>();

            ArgumentNullException.ThrowIfNull(kafkaSettings);

            builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection(KafkaSettings.SETTINGS_SECTION));

            var cacheSettings = builder.Configuration.GetSection(CacheSettings.SETTINGS_SECTION).Get<CacheSettings>();

            ArgumentNullException.ThrowIfNull(cacheSettings);

            builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection(CacheSettings.SETTINGS_SECTION));

            #endregion

            builder.Services.AddSignalR();

            #region Kafka

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                ClientId = kafkaSettings.ClientId,
                GroupId = kafkaSettings.GroupId,
                EnablePartitionEof = true,
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };

            var adminConfig = new AdminClientConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
            };

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                ClientId = kafkaSettings.ClientId,
                EnableIdempotence = true,
            };
            builder.Services.AddKafkaProducer(producerConfig, adminConfig);
            builder.Services.AddKafkaConsumer(consumerConfig, adminConfig);

            #endregion

            #region Caching

            builder.Services.AddStackExchangeRedisOutputCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString(CacheSettings.REDIS_SERVER_CONNECTION_STRING);
            });

            builder.Services.AddOutputCache((options) =>
            {
                options.AddPolicy("BasePolicy", new OutputCachePolicy());

                options.SetOutputCachePolicy("GetLoadEventsInDataRangePolicy", duration: TimeSpan.FromMinutes(cacheSettings.GetLoadEventsInDataRangeExpiryInMinutes), types: typeof(GetLoadEventsInDataRangeRequest));
                options.SetOutputCachePolicy("GetDailyLoadAmountStatisticsPolicy", duration: TimeSpan.FromMinutes(cacheSettings.GetDailyLoadAmountStatisticsExpiryInMinutes));
                options.SetOutputCachePolicy("GetLoadAmountStatisticsInRangePolicy", duration: TimeSpan.FromMinutes(cacheSettings.GetLoadAmountStatisticsInRangeExpiryInMinutes), types: typeof(GetLoadAmountStatisticsInRangeRequest));
                options.SetOutputCachePolicy("GetSlotStatisticsPolicy", duration: TimeSpan.FromMinutes(cacheSettings.GetSlotStatisticsExpiryInMinutes));
            });

            #endregion

            #region Project Services

            builder.Services.AddSingleton<IStatisticsNotifier<ServerLifecycleStatistics, ServerLifecycleStatisticsResponse>, SignalRStatisticsNotifier<ServerLifecycleStatistics, ServerLifecycleStatisticsResponse>>();
            builder.Services.AddSingleton<IStatisticsNotifier<ServerLoadStatistics, ServerLoadStatisticsResponse>, SignalRStatisticsNotifier<ServerLoadStatistics, ServerLoadStatisticsResponse>>();
            builder.Services.AddSingleton<IStatisticsNotifier<ServerCustomStatistics, ServerCustomStatisticsResponse>, SignalRStatisticsNotifier<ServerCustomStatistics, ServerCustomStatisticsResponse>>();

            #endregion

            return builder;
        }
    }
}
