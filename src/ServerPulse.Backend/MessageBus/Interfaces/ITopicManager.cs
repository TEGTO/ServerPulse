namespace MessageBus.Interfaces
{
    public interface ITopicManager
    {
        public Task CreateTopicsAsync(IEnumerable<string> topicList, int numPartitions = -1, short replicationFactor = -1, int timeoutInMilliseconds = 5000);
        public Task DeleteTopicsAsync(IEnumerable<string> topicList);
    }
}
