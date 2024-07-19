namespace TestKafka.Consumer.Services
{
    public interface IMessageConsumer<T>
    {
        public Task<List<T>> ReadMessagesInDateRangeAsync(string topicName, DateTime startDate, DateTime endDate, int timeoutInMilliseconds = 2000, CancellationToken cancellationToken = default!);
    }
}