using AnalyzerApi.Hubs;
using AnalyzerApi.Infrastructure;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Requests;
using AnalyzerApi.Infrastructure.Validators;
using AnalyzerApi.Infrastructure.Wrappers;
using AnalyzerApi.Services;
using AnalyzerApi.Services.Collectors;
using AnalyzerApi.Services.Consumers;
using AnalyzerApi.Services.Interfaces;
using AnalyzerApi.Services.Receivers.Event;
using AnalyzerApi.Services.Receivers.Statistics;
using Caching;
using Confluent.Kafka;
using EventCommunication.Events;
using EventCommunication.Validators;
using ExceptionHandling;
using Logging;
using MessageBus;
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
    AutoOffsetReset = AutoOffsetReset.Earliest
};
var adminConfig = new AdminClientConfig
{
    BootstrapServers = builder.Configuration[Configuration.KAFKA_BOOTSTRAP_SERVERS]
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

    options.SetOutputCachePolicy("GetLoadEventsInDataRangePolicy", duration: TimeSpan.FromMinutes(expiryTime), types: typeof(MessagesInRangeRangeRequest));
    options.SetOutputCachePolicy("GetWholeAmountStatisticsInDaysPolicy", duration: TimeSpan.FromMinutes(expiryTime));
    options.SetOutputCachePolicy("GetAmountStatisticsInRangePolicy", duration: TimeSpan.FromMinutes(expiryTime), types: typeof(MessageAmountInRangeRequest));
    options.SetOutputCachePolicy("GetSlotStatisticsPolicy", duration: TimeSpan.FromMinutes(expiryTime));
});

#endregion

#region Project Services

builder.Services.AddSingleton(new EventReceiverTopicData<ConfigurationEventWrapper>(builder.Configuration[Configuration.KAFKA_CONFIGURATION_TOPIC]!));
builder.Services.AddSingleton(new EventReceiverTopicData<CustomEventWrapper>(builder.Configuration[Configuration.KAFKA_CUSTOM_TOPIC]!));
builder.Services.AddSingleton(new EventReceiverTopicData<LoadEventWrapper>(builder.Configuration[Configuration.KAFKA_LOAD_TOPIC]!));
builder.Services.AddSingleton(new EventReceiverTopicData<PulseEventWrapper>(builder.Configuration[Configuration.KAFKA_ALIVE_TOPIC]!));

builder.Services.AddSingleton<IEventReceiver<ConfigurationEventWrapper>, EventReceiver<ConfigurationEvent, ConfigurationEventWrapper>>();
builder.Services.AddSingleton<IEventReceiver<CustomEventWrapper>, CustomEventReceiver>();
builder.Services.AddSingleton<IEventReceiver<LoadEventWrapper>, EventReceiver<LoadEvent, LoadEventWrapper>>();
builder.Services.AddSingleton<IEventReceiver<PulseEventWrapper>, EventReceiver<PulseEvent, PulseEventWrapper>>();

builder.Services.AddSingleton(new StatisticsReceiverTopicData<ServerCustomStatistics>(builder.Configuration[Configuration.KAFKA_CUSTOM_TOPIC]!));
builder.Services.AddSingleton(new StatisticsReceiverTopicData<LoadAmountStatistics>(builder.Configuration[Configuration.KAFKA_LOAD_TOPIC]!));
builder.Services.AddSingleton(new StatisticsReceiverTopicData<LoadMethodStatistics>(builder.Configuration[Configuration.KAFKA_LOAD_METHOD_STATISTICS_TOPIC]!));
builder.Services.AddSingleton(new StatisticsReceiverTopicData<ServerLoadStatistics>(builder.Configuration[Configuration.KAFKA_LOAD_TOPIC]!));
builder.Services.AddSingleton(new StatisticsReceiverTopicData<ServerStatistics>(builder.Configuration[Configuration.KAFKA_SERVER_STATISTICS_TOPIC]!));

builder.Services.AddSingleton<IStatisticsReceiver<ServerCustomStatistics>, StatisticsReceiver<ServerCustomStatistics>>();
builder.Services.AddSingleton<IStatisticsReceiver<LoadAmountStatistics>, LoadAmountStatisticsReceiver>();
builder.Services.AddSingleton<IStatisticsReceiver<LoadMethodStatistics>, StatisticsReceiver<LoadMethodStatistics>>();
builder.Services.AddSingleton<IStatisticsReceiver<ServerLoadStatistics>, StatisticsReceiver<ServerLoadStatistics>>();
builder.Services.AddSingleton<IStatisticsReceiver<ServerStatistics>, StatisticsReceiver<ServerStatistics>>();

builder.Services.AddSingleton<IStatisticsSender, StatisticsSender>();

builder.Services.AddSingleton<IStatisticsConsumer<ServerStatistics>, ServerStatisticsConsumer>();
builder.Services.AddSingleton<IStatisticsConsumer<ServerLoadStatistics>, StatisticsConsumer<ServerLoadStatistics, LoadEventWrapper>>();
builder.Services.AddSingleton<IStatisticsConsumer<ServerCustomStatistics>, StatisticsConsumer<ServerCustomStatistics, CustomEventWrapper>>();

builder.Services.AddSingleton<IStatisticsCollector<ServerStatistics>, ServerStatisticsCollector>();
builder.Services.AddSingleton<IStatisticsCollector<ServerLoadStatistics>, LoadStatisticsCollector>();
builder.Services.AddSingleton<IStatisticsCollector<ServerCustomStatistics>, CustomStatisticsCollector>();

#endregion

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddMediatR(conf =>
{
    conf.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddSharedFluentValidation(typeof(Program), typeof(GetSomeMessagesRequestValidator), typeof(ConfigurationEventValidator));

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwagger("Analyzer API");
}

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

app.MapHub<StatisticsHub<ServerStatistics>>("/statisticshub");
app.MapHub<StatisticsHub<ServerLoadStatistics>>("/loadstatisticshub");
app.MapHub<StatisticsHub<ServerCustomStatistics>>("/customstatisticshub");

#endregion

await app.RunAsync();

public partial class Program { }