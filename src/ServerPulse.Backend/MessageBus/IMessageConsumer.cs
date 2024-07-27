using System.Runtime.CompilerServices;

namespace TestKafka.Consumer.Services
{
    public interface IMessageConsumer
    {
        public IAsyncEnumerable<string> ConsumeAsync(string topic, int timeoutInMilliseconds, [EnumeratorCancellation] CancellationToken cancellationToken);
        public Task<List<string>> ReadMessagesInDateRangeAsync(string topicName, DateTime startDate, DateTime endDate, int timeoutInMilliseconds, CancellationToken cancellationToken);
        public Task<string?> ReadLastTopicMessageAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken);
        public Task<int> GetAmountTopicMessagesAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken);
        public Task<int> GetAmountTopicMessagesInDateRangeAsync(string topicName, DateTime startDate, DateTime endDate, int timeoutInMilliseconds, CancellationToken cancellationToken);
    }
}