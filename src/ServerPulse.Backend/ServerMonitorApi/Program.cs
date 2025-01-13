using Confluent.Kafka;
using EventCommunication;
using ExceptionHandling;
using Logging;
using MessageBus;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ServerMonitorApi.Options;
using ServerMonitorApi.Services;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1 * 1024 * 1024; //1 MB
});

builder.Services.AddHttpClientHelperServiceWithResilience(builder.Configuration);

#region Options

var messageBusSettings = builder.Configuration.GetSection(MessageBusSettings.SETTINGS_SECTION).Get<MessageBusSettings>();

ArgumentNullException.ThrowIfNull(messageBusSettings);

builder.Services.Configure<MessageBusSettings>(builder.Configuration.GetSection(MessageBusSettings.SETTINGS_SECTION));

var kafkaSettings = builder.Configuration.GetSection(KafkaSettings.SETTINGS_SECTION).Get<KafkaSettings>();

ArgumentNullException.ThrowIfNull(kafkaSettings);

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection(KafkaSettings.SETTINGS_SECTION));

#endregion

#region Kafka

var producerConfig = new ProducerConfig
{
    BootstrapServers = kafkaSettings.BootstrapServers,
    ClientId = kafkaSettings.ClientId,
    EnableIdempotence = true,
};
var adminConfig = new AdminClientConfig
{
    BootstrapServers = kafkaSettings.BootstrapServers,
    AllowAutoCreateTopics = true
};
builder.Services.AddKafkaProducer(producerConfig, adminConfig);

#endregion

#region Project Services

builder.Services.AddSingleton<ISlotKeyChecker, SlotKeyChecker>();

#endregion

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddSharedFluentValidation(typeof(Program), typeof(LoadEvent));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwagger("Server Monitor API");
}

var app = builder.Build();

app.UseSharedMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger("Server Monitor API V1");
}
else
{
    app.UseHttpsRedirection();
}

app.MapControllers();

await app.RunAsync();

public partial class Program { }