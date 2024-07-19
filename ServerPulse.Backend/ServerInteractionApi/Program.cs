using Confluent.Kafka;
using MessageBus;
using ServerInteractionApi;
using ServerInteractionApi.Services;

var builder = WebApplication.CreateBuilder(args);

var producerConfig = new ProducerConfig
{
    BootstrapServers = builder.Configuration[Configuration.KAFKA_BOOTSTRAP_SERVERS],
    ClientId = builder.Configuration[Configuration.KAFKA_CLIENT_ID],
    EnableIdempotence = true,

};
builder.Services.AddSingleton<IMessageProducer>(new KafkaProducer(producerConfig));

builder.Services.AddSingleton<IMessageSender, MessageSender>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();