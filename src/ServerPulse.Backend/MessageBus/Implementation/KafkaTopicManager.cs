using Confluent.Kafka;
using Confluent.Kafka.Admin;
using MessageBus.Interfaces;

namespace MessageBus.Kafka
{
    internal sealed class KafkaTopicManager : ITopicManager
    {
        private readonly IAdminClient adminClient;

        public KafkaTopicManager(IAdminClient adminClient)
        {
            this.adminClient = adminClient;
        }

        public async Task CreateTopicsAsync(IEnumerable<string> topicList, int numPartitions = -1, short replicationFactor = -1, int timeoutInMilliseconds = 5000)
        {
            var existingTopics = GetExistingTopics();

            var topicsToCreate = topicList
                .Where(topic => !existingTopics.Contains(topic))
                .Select(topic => new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = numPartitions,
                    ReplicationFactor = replicationFactor
                })
                .ToList();

            if (topicsToCreate.Any())
            {
                var options = new CreateTopicsOptions() { OperationTimeout = TimeSpan.FromMilliseconds(timeoutInMilliseconds) };
                await adminClient.CreateTopicsAsync(topicsToCreate, options);
            }
        }

        public async Task DeleteTopicsAsync(IEnumerable<string> topicList)
        {
            var existingTopics = GetExistingTopics();

            var topicsToDelete = topicList.Where(existingTopics.Contains);
            if (topicsToDelete.Any())
            {
                await adminClient.DeleteTopicsAsync(topicsToDelete);
            }
        }

        private IList<string> GetExistingTopics()
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            return metadata.Topics.Select(t => t.Topic).ToList();
        }
    }
}