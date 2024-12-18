#pragma warning disable CS8424 
using Confluent.Kafka;
using MessageBus.Models;
using System.Runtime.CompilerServices;

namespace MessageBus.Interfaces
{

    public interface IMessageConsumer
    {
        public IAsyncEnumerable<ConsumeResponse> ConsumeAsync(string topic, int timeoutInMilliseconds, Offset consumeFrom, [EnumeratorCancellation] CancellationToken cancellationToken);
        public Task<ConsumeResponse?> GetLastTopicMessageAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken);
        public Task<IEnumerable<ConsumeResponse>> GetMessagesInDateRangeAsync(GetMessageInDateRangeOptions options, CancellationToken cancellationToken);
        public Task<int> GetTopicMessageAmountAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken);
        public Task<Dictionary<DateTime, int>> GetTopicMessageAmountPerTimespanAsync(GetMessageInDateRangeOptions options, TimeSpan timeSpan, CancellationToken cancellationToken);
        public Task<IEnumerable<ConsumeResponse>> GetSomeMessagesStartFromDateAsync(GetSomeMessagesFromDateOptions options, CancellationToken cancellationToken);
    }
}