namespace TestKafka.Consumer.Services
{
    public interface IMessageConsumer
    {
        public Task<List<string>> ReadMessagesInDateRangeAsync(string topicName, DateTime startDate, DateTime endDate, int timeoutInMilliseconds = 2000, CancellationToken cancellationToken = default!);
    }
}