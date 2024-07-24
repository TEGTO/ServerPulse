using AnalyzerApi;
using AnalyzerApi.Services;
using Confluent.Kafka;
using FluentValidation;
using MessageBus.Kafka;
using Shared;
using Shared.Middlewares;
using TestKafka.Consumer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var consumerConfig = new ConsumerConfig
{
    BootstrapServers = builder.Configuration[Configuration.KAFKA_BOOTSTRAP_SERVERS],
    ClientId = builder.Configuration[Configuration.KAFKA_CLIENT_ID],
    GroupId = builder.Configuration[Configuration.KAFKA_GROUP_ID],
};
var adminConfig = new AdminClientConfig
{
    BootstrapServers = builder.Configuration[Configuration.KAFKA_BOOTSTRAP_SERVERS]
};
builder.Services.AddSingleton(consumerConfig);
builder.Services.AddSingleton(new AdminClientBuilder(adminConfig).Build());
builder.Services.AddSingleton<IKafkaConsumerFactory, KafkaConsumerFactory>();
builder.Services.AddSingleton<IMessageConsumer, KafkaConsumer>();
builder.Services.AddSingleton<IMessageReceiver, MessageReceiver>();
builder.Services.AddSingleton<IServerAnalyzer, ServerAnalyzer>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
ValidatorOptions.Global.LanguageManager.Enabled = false;

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

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