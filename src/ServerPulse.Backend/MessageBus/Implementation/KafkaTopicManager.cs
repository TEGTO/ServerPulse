using Confluent.Kafka;
using MessageBus.Interfaces;

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
            var existingTopics = GetExistingTopicsAsync();

            var topicsToDelete = topicList.Where(existingTopics.Contains);
            if (topicsToDelete.Any())
            {
                await adminClient.DeleteTopicsAsync(topicsToDelete);
            }
        }
        private IList<string> GetExistingTopicsAsync()
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            return metadata.Topics.Select(t => t.Topic).ToList();
        }
    }
}