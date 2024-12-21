using AnalyzerApi.BackgroundServices;
using AnalyzerApi.Hubs;
using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Configurations;
using AnalyzerApi.Infrastructure.Models.Statistics;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Infrastructure.Requests;
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

#region Kafka

var consumerConfig = new ConsumerConfig
{
    BootstrapServers = builder.Configuration[Configuration.KAFKA_BOOTSTRAP_SERVERS],
    ClientId = builder.Configuration[Configuration.KAFKA_CLIENT_ID],
    GroupId = builder.Configuration[Configuration.KAFKA_GROUP_ID],
    EnablePartitionEof = true,
    AutoOffsetReset = AutoOffsetReset.Earliest,
};

var adminConfig = new AdminClientConfig
{
    BootstrapServers = builder.Configuration[Configuration.KAFKA_BOOTSTRAP_SERVERS],
};

var producerConfig = new ProducerConfig
{
    BootstrapServers = builder.Configuration[Configuration.KAFKA_BOOTSTRAP_SERVERS],
    ClientId = builder.Configuration[Configuration.KAFKA_CLIENT_ID],
    EnableIdempotence = true,
};
builder.Services.AddKafkaProducer(producerConfig, adminConfig);
builder.Services.AddKafkaConsumer(consumerConfig, adminConfig);

#endregion 

#region Caching

builder.Services.AddOutputCache((options) =>
{
    options.AddPolicy("BasePolicy", new OutputCachePolicy());

    var expiryTime = int.TryParse(
        builder.Configuration[Configuration.CACHE_EXPIRY_IN_MINUTES],
        out var getByEmailExpiry) ? getByEmailExpiry : 1;

    options.SetOutputCachePolicy("GetLoadEventsInDataRangePolicy", duration: TimeSpan.FromMinutes(expiryTime), types: typeof(MessagesInRangeRequest));
    options.SetOutputCachePolicy("GetDailyLoadStatisticsPolicy", duration: TimeSpan.FromMinutes(expiryTime));
    options.SetOutputCachePolicy("GetLoadAmountStatisticsInRangePolicy", duration: TimeSpan.FromMinutes(expiryTime), types: typeof(MessageAmountInRangeRequest));
    options.SetOutputCachePolicy("GetSlotStatisticsPolicy", duration: TimeSpan.FromMinutes(expiryTime));
});

#endregion

#region Resilience

builder.Services.AddResiliencePipeline(Configuration.LOAD_EVENT_PROCESSING_RESILLIENCE, (builder, context) =>
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

builder.Services.AddSingleton(new EventReceiverTopicConfiguration<ConfigurationEventWrapper>(builder.Configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]!));
builder.Services.AddSingleton(new EventReceiverTopicConfiguration<CustomEventWrapper>(builder.Configuration[Configuration.KAFKA_CUSTOM_TOPIC]!));
builder.Services.AddSingleton(new EventReceiverTopicConfiguration<LoadEventWrapper>(builder.Configuration[Configuration.KAFKA_LOAD_TOPIC]!));
builder.Services.AddSingleton(new EventReceiverTopicConfiguration<PulseEventWrapper>(builder.Configuration[Configuration.KAFKA_ALIVE_TOPIC]!));

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

builder.Services.AddSingleton(new StatisticsReceiverTopicConfiguration<ServerCustomStatistics>(builder.Configuration[Configuration.KAFKA_CUSTOM_TOPIC]!));
builder.Services.AddSingleton(new StatisticsReceiverTopicConfiguration<LoadAmountStatistics>(builder.Configuration[Configuration.KAFKA_LOAD_TOPIC]!));
builder.Services.AddSingleton(new StatisticsReceiverTopicConfiguration<LoadMethodStatistics>(builder.Configuration[Configuration.KAFKA_LOAD_METHOD_STATISTICS_TOPIC]!));
builder.Services.AddSingleton(new StatisticsReceiverTopicConfiguration<ServerLoadStatistics>(builder.Configuration[Configuration.KAFKA_LOAD_TOPIC]!));
builder.Services.AddSingleton(new StatisticsReceiverTopicConfiguration<ServerLifecycleStatistics>(builder.Configuration[Configuration.KAFKA_SERVER_STATISTICS_TOPIC]!));

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

builder.Services.AddSharedFluentValidation(typeof(Program), typeof(GetSomeMessagesRequestValidator), typeof(ConfigurationEventValidator));

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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseSwagger("Analyzer API V1");
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