using Confluent.Kafka;

namespace MessageBus.Kafka
{
    public class KafkaTopicManager : ITopicManager
    {
        private readonly IAdminClient adminClient;

        public KafkaTopicManager(IAdminClient adminClient)
        {
            this.adminClient = adminClient;
        }

        public async Task DeleteTopicsAsync(IEnumerable<string> topicList)
        {
            await adminClient.DeleteTopicsAsync(topicList, null);
        }
    }
}
