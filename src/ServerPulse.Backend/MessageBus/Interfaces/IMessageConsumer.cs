#pragma warning disable CS8424 
using Confluent.Kafka;
using System.Runtime.CompilerServices;

namespace MessageBus.Interfaces
{
    public record MessageInRangeQueryOptions(string TopicName, int TimeoutInMilliseconds, DateTime From, DateTime To);
    public record ReadSomeMessagesOptions(string TopicName, int TimeoutInMilliseconds, int NumberOfMessages, DateTime StartDate, bool ReadNew = false);
    public record ConsumeResponse(string Message, DateTime CreationTimeUTC);

    public interface IMessageConsumer
    {
        public IAsyncEnumerable<ConsumeResponse> ConsumeAsync(string topic, int timeoutInMilliseconds, Offset consumeFrom, [EnumeratorCancellation] CancellationToken cancellationToken);
        public Task<ConsumeResponse?> ReadLastTopicMessageAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken);
        public Task<List<ConsumeResponse>> ReadMessagesInDateRangeAsync(MessageInRangeQueryOptions options, CancellationToken cancellationToken);
        public Task<int> GetAmountTopicMessagesAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken);
        public Task<Dictionary<DateTime, int>> GetMessageAmountPerTimespanAsync(MessageInRangeQueryOptions options, TimeSpan timeSpan, CancellationToken cancellationToken);
        public Task<List<ConsumeResponse>> ReadSomeMessagesAsync(ReadSomeMessagesOptions options, CancellationToken cancellationToken);
    }
}