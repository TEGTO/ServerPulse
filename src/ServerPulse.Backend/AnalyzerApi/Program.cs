using AnalyzerApi;
using AnalyzerApi.Hubs;
using AnalyzerApi.Services;
using AnalyzerApi.Services.Collectors;
using AnalyzerApi.Services.Interfaces;
using AnalyzerApi.Services.Receivers;
using CacheUtils;
using Confluent.Kafka;
using ConsulUtils.Extension;
using MessageBus;
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

builder.Services.AddSingleton<IServerStatusReceiver, ServerStatusReceiver>();
builder.Services.AddSingleton<IServerLoadReceiver, ServerLoadReceiver>();
builder.Services.AddSingleton<ICustomReceiver, CustomReceiver>();
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