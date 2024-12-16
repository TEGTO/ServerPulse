using AnalyzerApi.Command.BackgroundServices.ProcessLoadEvents;
using AnalyzerApi.Infrastructure;
using Confluent.Kafka;
using EventCommunication;
using MediatR;
using MessageBus.Interfaces;
using Shared;

namespace AnalyzerApi.BackgroundServices
{
    public class LoadEventStatisticsProcessor : BackgroundService
    {
        private readonly IMessageConsumer messageConsumer;
        private readonly IMediator mediator;
        private readonly ITopicManager topicManager;
        private readonly string topic;
        private readonly int timeoutInMilliseconds;

        public LoadEventStatisticsProcessor(
            IMessageConsumer messageConsumer,
            IMediator mediator,
            ITopicManager topicManager,
            IConfiguration configuration)
        {
            this.messageConsumer = messageConsumer;
            this.mediator = mediator;
            this.topicManager = topicManager;
            topic = configuration[Configuration.KAFKA_LOAD_TOPIC_PROCESS]!;
            timeoutInMilliseconds = int.Parse(configuration[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]!);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await topicManager.CreateTopicsAsync([topic], timeoutInMilliseconds: timeoutInMilliseconds);

            await foreach (var response in messageConsumer.ConsumeAsync(topic, timeoutInMilliseconds, Offset.Stored, stoppingToken))
            {
                if (response.Message.TryToDeserialize(out LoadEvent? ev) && ev != null)
                {
                    await mediator.Send(new ProcessLoadEventsCommand([ev]), stoppingToken);
                }
            }
        }
    }
}
