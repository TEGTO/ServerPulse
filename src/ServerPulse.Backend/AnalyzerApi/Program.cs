using AnalyzerApi;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Hubs;
using AnalyzerApi.Services;
using AnalyzerApi.Services.Collectors;
using AnalyzerApi.Services.Interfaces;
using AnalyzerApi.Services.Receivers.Event;
using AnalyzerApi.Services.Receivers.Statistics;
using CacheUtils;
using Confluent.Kafka;
using ConsulUtils.Extension;
using MessageBus;
using ServerPulse.EventCommunication.Events;
using Shared;
using Shared.Middlewares;

var builder = WebApplication.CreateBuilder(args);

#region Consul

string environmentName = builder.Environment.EnvironmentName;
builder.Services.AddHealthChecks();
var consulSettings = ConsulExtension.GetConsulSettings(builder.Configuration);
builder.Services.AddConsulService(consulSettings);
builder.Configuration.ConfigureConsul(consulSettings, environmentName);

#endregion 

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

builder.Services.AddCache(builder.Configuration);

#region Project Services

builder.Services.AddSingleton(new EventReceiverTopicData<ConfigurationEventWrapper>(Configuration.KAFKA_CONFIGURATION_TOPIC));
builder.Services.AddSingleton(new EventReceiverTopicData<CustomEventWrapper>(Configuration.KAFKA_CUSTOM_TOPIC));
builder.Services.AddSingleton(new EventReceiverTopicData<LoadEventWrapper>(Configuration.KAFKA_LOAD_TOPIC));
builder.Services.AddSingleton(new EventReceiverTopicData<PulseEventWrapper>(Configuration.KAFKA_ALIVE_TOPIC));

builder.Services.AddSingleton<IEventReceiver<ConfigurationEventWrapper>, EventReceiver<ConfigurationEvent, ConfigurationEventWrapper>>();
builder.Services.AddSingleton<IEventReceiver<CustomEventWrapper>, CustomEventReceiver>();
builder.Services.AddSingleton<IEventReceiver<LoadEventWrapper>, EventReceiver<LoadEvent, LoadEventWrapper>>();
builder.Services.AddSingleton<IEventReceiver<PulseEventWrapper>, EventReceiver<PulseEvent, PulseEventWrapper>>();

builder.Services.AddSingleton(new StatisticsReceiverTopicData<CustomEventStatistics>(Configuration.KAFKA_CUSTOM_TOPIC));
builder.Services.AddSingleton(new StatisticsReceiverTopicData<LoadAmountStatistics>(Configuration.KAFKA_LOAD_TOPIC));
builder.Services.AddSingleton(new StatisticsReceiverTopicData<LoadMethodStatistics>(Configuration.KAFKA_LOAD_METHOD_STATISTICS_TOPIC));
builder.Services.AddSingleton(new StatisticsReceiverTopicData<ServerLoadStatistics>(Configuration.KAFKA_LOAD_TOPIC));
builder.Services.AddSingleton(new StatisticsReceiverTopicData<ServerStatistics>(Configuration.KAFKA_LOAD_TOPIC));

builder.Services.AddSingleton<IStatisticsReceiver<CustomEventStatistics>, StatisticsReceiver<CustomEventStatistics>>();
builder.Services.AddSingleton<IStatisticsReceiver<LoadAmountStatistics>, LoadAmountStatisticsReceiver>();
builder.Services.AddSingleton<IStatisticsReceiver<LoadMethodStatistics>, StatisticsReceiver<LoadMethodStatistics>>();
builder.Services.AddSingleton<IStatisticsReceiver<ServerLoadStatistics>, StatisticsReceiver<ServerLoadStatistics>>();
builder.Services.AddSingleton<IStatisticsReceiver<ServerStatistics>, StatisticsReceiver<ServerStatistics>>();


builder.Services.AddSingleton<IStatisticsSender, StatisticsSender>();
builder.Services.AddSingleton<IEventProcessor, EventProcessor>();
builder.Services.AddSingleton<ServerStatisticsCollector>();
builder.Services.AddSingleton<LoadStatisticsCollector>();
builder.Services.AddSingleton<CustomStatisticsCollector>();

#endregion

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddSharedFluentValidation(typeof(Program));

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR();

var app = builder.Build();

app.UseExceptionMiddleware();

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

#region Hubs

app.MapHub<StatisticsHub<ServerStatisticsCollector>>("/statisticshub");
app.MapHub<StatisticsHub<LoadStatisticsCollector>>("/loadstatisticshub");
app.MapHub<StatisticsHub<CustomStatisticsCollector>>("/customstatisticshub");

#endregion

app.Run();