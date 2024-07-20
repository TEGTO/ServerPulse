using Confluent.Kafka;
using FluentValidation;
using MessageBus;
using ServerInteractionApi;
using ServerInteractionApi.Services;
using Shared;
using Shared.Middlewares;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));
builder.Services.AddHttpClient();

var producerConfig = new ProducerConfig
{
    BootstrapServers = builder.Configuration[Configuration.KAFKA_BOOTSTRAP_SERVERS],
    ClientId = builder.Configuration[Configuration.KAFKA_CLIENT_ID],
    EnableIdempotence = true,

};
builder.Services.AddSingleton<IMessageProducer>(new KafkaProducer(producerConfig));
builder.Services.AddSingleton<IMessageSender, MessageSender>();

builder.Services.AddSingleton<IServerSlotChecker, ServerSlotChecker>();

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
ValidatorOptions.Global.LanguageManager.Enabled = false;


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