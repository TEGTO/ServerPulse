using EventCommunication.Events;
using System.Text.Json;
using TestKafka.Consumer.Services;

namespace AnalyzerApi.Services
{
    public class MessageReceiver : IMessageReceiver
    {
        private readonly IMessageConsumer messageConsumer;
        private readonly string aliveTopic;

        public MessageReceiver(IMessageConsumer messageConsumer, IConfiguration configuration)
        {
            this.messageConsumer = messageConsumer;
            aliveTopic = configuration[Configuration.KAFKA_ALIVE_TOPIC]!;
        }

        public async Task<AliveEvent> ReceiveLastAliveEventByKeyAsync(string key)
        {
            string topic = aliveTopic.Replace("{id}", key);
            string? message = await messageConsumer.ReadLastTopicMessageAsync(topic);
            AliveEvent alive = new AliveEvent(key, false);
            if (!string.IsNullOrEmpty(message))
            {
                alive = JsonSerializer.Deserialize<AliveEvent>(message);
            }
            return alive;
        }
    }
}