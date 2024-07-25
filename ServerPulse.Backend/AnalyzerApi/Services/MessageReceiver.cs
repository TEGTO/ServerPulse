using EventCommunication.Events;
using System.Text.Json;
using TestKafka.Consumer.Services;

namespace AnalyzerApi.Services
{
    public class MessageReceiver : IMessageReceiver
    {
        private readonly IMessageConsumer messageConsumer;
        private readonly string aliveTopic;
        private readonly int timeoutInMilliseconds;

        public MessageReceiver(IMessageConsumer messageConsumer, IConfiguration configuration)
        {
            this.messageConsumer = messageConsumer;
            aliveTopic = configuration[Configuration.KAFKA_ALIVE_TOPIC]!;
            timeoutInMilliseconds = int.Parse(configuration[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]!);
        }

        public async Task<AliveEvent> ReceiveLastAliveEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = aliveTopic.Replace("{id}", key);
            string? message = await messageConsumer.ReadLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);
            AliveEvent alive = new AliveEvent(key, false);
            if (!string.IsNullOrEmpty(message))
            {
                alive = JsonSerializer.Deserialize<AliveEvent>(message);
            }
            return alive;
        }
    }
}