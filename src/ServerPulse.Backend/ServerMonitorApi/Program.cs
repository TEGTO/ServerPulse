using Confluent.Kafka;
using EventCommunication;
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

#region Project Services

builder.Services.AddSingleton<ISlotKeyChecker, SlotKeyChecker>();
builder.Services.AddSingleton<IStatisticsEventSender, StatisticsEventSender>();

#endregion

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddMediatR(conf =>
{
    conf.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddSharedFluentValidation(typeof(Program), typeof(LoadEvent));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwagger("Server Monitor API");
}

var app = builder.Build();

app.UseSharedMiddleware();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseSwagger("Server Monitor API V1");
}

app.MapControllers();

await app.RunAsync();

public partial class Program { }