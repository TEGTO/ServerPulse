namespace MessageBus
{
    public interface ITopicManager
    {
        public Task DeleteTopicsAsync(IEnumerable<string> topicList);
    }
}
