using Confluent.Kafka;
using FluentValidation;
using MessageBus;
using MessageBus.Kafka;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ServerMonitorApi;
using ServerMonitorApi.Services;
using Shared;
using Shared.Middlewares;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1 * 1024 * 1024; //1 MB
});

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString(Configuration.REDIS_CONNECTION_STRING)!));
builder.Services.AddHttpClient();
 
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
builder.Services.AddSingleton(producerConfig);
builder.Services.AddSingleton(new AdminClientBuilder(adminConfig).Build());
builder.Services.AddSingleton<IKafkaProducerFactory, KafkaProducerFactory>();
builder.Services.AddSingleton<IMessageProducer, KafkaProducer>();
builder.Services.AddSingleton<ITopicManager, KafkaTopicManager>();

builder.Services.AddSingleton<IMessageSender, MessageSender>();
builder.Services.AddSingleton<IStatisticsControlService, StatisticsControlService>();

builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddSingleton<ISlotKeyChecker, SlotKeyChecker>();

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
ValidatorOptions.Global.LanguageManager.Enabled = false;

var app = builder.Build();

app.UseHttpsRedirection();
app.UseExceptionMiddleware();

app.UseAuthorization();

app.MapControllers();

app.Run();