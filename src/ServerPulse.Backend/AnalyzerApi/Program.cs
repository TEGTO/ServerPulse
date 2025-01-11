using AnalyzerApi.BackgroundServices;
using AnalyzerApi.Hubs;
using AnalyzerApi.Infrastructure.Configuration;
using AnalyzerApi.Infrastructure.Dtos.Endpoints.Analyze.GetLoadAmountStatisticsInRange;
using AnalyzerApi.Infrastructure.Dtos.Endpoints.Analyze.GetLoadEventsInDataRange;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Infrastructure.TopicMapping;
using AnalyzerApi.Infrastructure.Validators;
using AnalyzerApi.Services.Receivers.Event;
using AnalyzerApi.Services.Receivers.Statistics;
using AnalyzerApi.Services.SerializeStrategies;
using AnalyzerApi.Services.StatisticsDispatchers;
using Caching;
using Confluent.Kafka;
using EventCommunication.Validators;
using ExceptionHandling;
using Logging;
using MessageBus;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

#region Options

var messageBusSettings = builder.Configuration.GetSection(MessageBusSettings.SETTINGS_SECTION).Get<MessageBusSettings>();

ArgumentNullException.ThrowIfNull(messageBusSettings);

builder.Services.Configure<MessageBusSettings>(builder.Configuration.GetSection(MessageBusSettings.SETTINGS_SECTION));

var processSettings = builder.Configuration.GetSection(LoadProcessingSettings.SETTINGS_SECTION).Get<LoadProcessingSettings>();

ArgumentNullException.ThrowIfNull(processSettings);

builder.Services.Configure<LoadProcessingSettings>(builder.Configuration.GetSection(LoadProcessingSettings.SETTINGS_SECTION));

var cacheSettings = builder.Configuration.GetSection(CacheSettings.SETTINGS_SECTION).Get<CacheSettings>();

ArgumentNullException.ThrowIfNull(cacheSettings);

builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection(CacheSettings.SETTINGS_SECTION));

#endregion

#region Kafka

var consumerConfig = new ConsumerConfig
{
    BootstrapServers = messageBusSettings.BootstrapServers,
    ClientId = messageBusSettings.ClientId,
    GroupId = messageBusSettings.GroupId,
    EnablePartitionEof = true,
    AutoOffsetReset = AutoOffsetReset.Earliest,
};

var adminConfig = new AdminClientConfig
{
    BootstrapServers = messageBusSettings.BootstrapServers,
};

var producerConfig = new ProducerConfig
{
    BootstrapServers = messageBusSettings.BootstrapServers,
    ClientId = messageBusSettings.ClientId,
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

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddSharedFluentValidation(typeof(Program), typeof(GetSomeLoadEventsRequestValidator), typeof(ConfigurationEventValidator));

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwagger("Analyzer API");
}

builder.Services.AddHostedService<LoadEventStatisticsProcessor>();

var app = builder.Build();

app.UseSharedMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger("Analyzer API V1");
}
else
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.UseOutputCache(); //Order after Identity

#region Hubs

app.MapHub<StatisticsHub<ServerLifecycleStatistics>>("/lifecyclestatisticshub");
app.MapHub<StatisticsHub<ServerLoadStatistics>>("/loadstatisticshub");
app.MapHub<StatisticsHub<ServerCustomStatistics>>("/customstatisticshub");

#endregion

await app.RunAsync();

public partial class Program { }