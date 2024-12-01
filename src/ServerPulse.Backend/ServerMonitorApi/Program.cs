using Authentication;
using CacheUtils;
using Confluent.Kafka;
using ExceptionHandling;
using Logging;
using MessageBus;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ServerMonitorApi;
using ServerMonitorApi.Services;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1 * 1024 * 1024; //1 MB
});

builder.Services.AddCustomHttpClientServiceWithResilience(builder.Configuration);

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

app.UseSharedMiddleware();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseIdentity();

app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync();

public partial class Program { }