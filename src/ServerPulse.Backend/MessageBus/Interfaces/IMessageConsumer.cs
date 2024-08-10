using Confluent.Kafka;
using MessageBus.Kafka;
using System.Runtime.CompilerServices;

namespace MessageBus.Interfaces
{
    public interface IMessageConsumer
    {
        public IAsyncEnumerable<string> ConsumeAsync(string topic, int timeoutInMilliseconds, Offset consumeFrom, [EnumeratorCancellation] CancellationToken cancellationToken);
        public Task<string?> ReadLastTopicMessageAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken);
        public Task<List<string>> ReadMessagesInDateRangeAsync(MessageInRangeQueryOptions options, CancellationToken cancellationToken);
        public Task<int> GetAmountTopicMessagesAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken);
        public Task<Dictionary<DateTime, int>> GetMessageAmountPerTimespanAsync(MessageInRangeQueryOptions options, TimeSpan timeSpan, CancellationToken cancellationToken);
    }
}