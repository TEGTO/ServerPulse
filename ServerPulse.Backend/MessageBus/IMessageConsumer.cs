using System.Runtime.CompilerServices;

namespace TestKafka.Consumer.Services
{
    public interface IMessageConsumer
    {
        public IAsyncEnumerable<string> ConsumeAsync(string topic, int timeoutInMilliseconds, [EnumeratorCancellation] CancellationToken cancellationToken);
        public Task<List<string>> ReadMessagesInDateRangeAsync(string topicName, DateTime startDate, DateTime endDate, int timeoutInMilliseconds = 2000, CancellationToken cancellationToken = default!);
        public Task<string?> ReadLastTopicMessageAsync(string topicName, int timeoutInMilliseconds = 2000, CancellationToken cancellationToken = default!);
    }
}