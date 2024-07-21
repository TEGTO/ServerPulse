using Confluent.Kafka;
using FluentValidation;
using MessageBus;
using ServerInteractionApi;
using ServerInteractionApi.Services;
using Shared;
using Shared.Middlewares;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString(Configuration.REDIS_CONNECTION_STRING)));
builder.Services.AddHttpClient();

var producerConfig = new ProducerConfig
{
    BootstrapServers = builder.Configuration[Configuration.KAFKA_BOOTSTRAP_SERVERS],
    ClientId = builder.Configuration[Configuration.KAFKA_CLIENT_ID],
    EnableIdempotence = true,

};
builder.Services.AddSingleton<IMessageProducer>(new KafkaProducer(producerConfig));
builder.Services.AddSingleton<IMessageSender, MessageSender>();

builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddSingleton<ISlotKeyChecker, SlotKeyChecker>();

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
ValidatorOptions.Global.LanguageManager.Enabled = false;


builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<SlotKeyRedisDelete>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionMiddleware();

app.UseAuthorization();

app.MapControllers();

app.Run();