using CacheUtils;
using Confluent.Kafka;
using ConsulUtils.Extension;
using MessageBus;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ServerMonitorApi;
using ServerMonitorApi.Services;
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

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1 * 1024 * 1024; //1 MB
});

builder.Services.AddHttpClient();

#region Kafka

var producerConfig = new ProducerConfig
{
    BootstrapServers = builder.Configuration[Configuration.KAFKA_BOOTSTRAP_SERVERS],
    ClientId = builder.Configuration[Configuration.KAFKA_CLIENT_ID],
    EnableIdempotence = true,
};
var adminConfig = new AdminClientConfig
{
    BootstrapServers = builder.Configuration[Configuration.KAFKA_BOOTSTRAP_SERVERS]
};
builder.Services.AddKafkaProducer(producerConfig, adminConfig);

#endregion

#region Cache

builder.Services.AddCache(builder.Configuration);

#endregion

#region Project Services

builder.Services.AddSingleton<IEventSender, EventSender>();
builder.Services.AddSingleton<IStatisticsControlService, StatisticsControlService>();
builder.Services.AddSingleton<IEventProcessing, EventProcessing>();
builder.Services.AddSingleton<ISlotKeyChecker, SlotKeyChecker>();

#endregion

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddSharedFluentValidation(typeof(Program));

var app = builder.Build();

app.UseExceptionMiddleware();

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program { }