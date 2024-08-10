namespace MessageBus.Interfaces
{
    public interface ITopicManager
    {
        public Task DeleteTopicsAsync(IEnumerable<string> topicList);
    }
}
