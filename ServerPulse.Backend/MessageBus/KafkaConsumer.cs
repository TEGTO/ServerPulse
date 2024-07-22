using Confluent.Kafka;
using MessageBus;
using System.Runtime.CompilerServices;

namespace TestKafka.Consumer.Services
{
    public class KafkaConsumer : IMessageConsumer
    {
        private readonly IAdminClient adminClient;
        private readonly IConsumerFactory consumerFactory;

        public KafkaConsumer(IAdminClient adminClient, IConsumerFactory consumerFactory)
        {
            this.adminClient = adminClient;
            this.consumerFactory = consumerFactory;
        }

        public async IAsyncEnumerable<string> ConsumeAsync(string topic, int timeoutInMilliseconds, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using (var consumer = consumerFactory.CreateConsumer())
            {
                consumer.Subscribe(topic);
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(timeoutInMilliseconds));
                    if (consumeResult != null && !string.IsNullOrEmpty(consumeResult.Message.Value))
                    {
                        yield return consumeResult.Message.Value;
                    }
                    await Task.Yield();
                }
            }
        }

        public async Task<string?> ReadLastTopicMessageAsync(string topicName, int timeoutInMilliseconds = 2000, CancellationToken cancellationToken = default!)
        {
            var val = await Task.Run(() => ReadLastTopicMessage(topicName, timeoutInMilliseconds, cancellationToken));
            return val;
        }

        private string? ReadLastTopicMessage(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            using (var consumer = consumerFactory.CreateConsumer())
            {
                consumer.Subscribe(topicName);
                var topicMetadata = adminClient.GetMetadata(topicName, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
                var partitionMetadatas = topicMetadata.Topics.FirstOrDefault(x => x.Topic == topicName)?.Partitions;

                if (partitionMetadatas == null)
                {
                    return null;
                }

                var tasks = partitionMetadatas.Select(partition => Task.Run(() =>
                    ReadPartitionLatestMessage(consumer, topicName, partition.PartitionId, timeoutInMilliseconds, cancellationToken)
                )).ToList();

                Task.WaitAll(tasks.ToArray(), cancellationToken);

                var latestMessage = tasks
                    .Select(task => task.Result)
                    .Where(result => result != null)
                    .OrderByDescending(result => result.Message.Timestamp.UtcDateTime)
                    .FirstOrDefault();

                return latestMessage?.Message.Value;
            }
        }

        private ConsumeResult<string, string>? ReadPartitionLatestMessage(IConsumer<string, string> consumer, string topicName, int partitionId, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            var partition = new TopicPartition(topicName, partitionId);
            var watermarkOffsets = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
            var lastOffset = watermarkOffsets.High - 1;

            if (lastOffset < 0)
                return null; // Skip empty partitions

            consumer.Assign(new List<TopicPartitionOffset> { new TopicPartitionOffset(partition, lastOffset) });
            consumer.Seek(new TopicPartitionOffset(partition, lastOffset));

            var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(timeoutInMilliseconds));
            if (consumeResult == null || string.IsNullOrEmpty(consumeResult.Message.Value))
                return null;

            return consumeResult;
        }

        public async Task<List<string>> ReadMessagesInDateRangeAsync(string topicName, DateTime startDate, DateTime endDate, int timeoutInMilliseconds = 2000, CancellationToken cancellationToken = default!)
        {
            var val = await Task.Run(() => ReadMessagesInDateRange(topicName, startDate, endDate, timeoutInMilliseconds, cancellationToken));
            return val;
        }

        private List<string> ReadMessagesInDateRange(string topicName, DateTime startDate, DateTime endDate, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            if (startDate.ToUniversalTime() >= DateTime.UtcNow || startDate.ToUniversalTime() >= endDate.ToUniversalTime())
            {
                throw new Exception("Invalid Start Date! Must be less than now (UTC) and End Date!");
            }

            var messages = new List<string>();
            using (var consumer = consumerFactory.CreateConsumer())
            {
                consumer.Subscribe(topicName);
                var topicMetadata = adminClient.GetMetadata(topicName, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
                var partitionMetadatas = topicMetadata.Topics.First(x => x.Topic == topicName).Partitions;
                var topicPartitions = partitionMetadatas.Select(p => new TopicPartition(topicName, p.PartitionId)).ToList();

                var startTimestamps = topicPartitions.Select(tp => new TopicPartitionTimestamp(tp, new Timestamp(startDate.ToUniversalTime()))).ToList();
                var endTimestamps = topicPartitions.Select(tp => new TopicPartitionTimestamp(tp, new Timestamp(endDate.ToUniversalTime()))).ToList();
                var startOffsets = consumer.OffsetsForTimes(startTimestamps, TimeSpan.FromMilliseconds(timeoutInMilliseconds)).ToList();
                var endOffsets = consumer.OffsetsForTimes(endTimestamps, TimeSpan.FromMilliseconds(timeoutInMilliseconds)).ToList();
                consumer.Assign(startOffsets);

                foreach (var startOffset in startOffsets)
                {
                    consumer.Seek(startOffset);
                    var endOffset = endOffsets.First(e => e.TopicPartition == startOffset.TopicPartition);
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var consumeResult = consumer.Consume(timeoutInMilliseconds);
                        if (consumeResult == null)
                            break;
                        if (string.IsNullOrEmpty(consumeResult.Message.Value))
                            continue;

                        if (consumeResult.Message.Timestamp.UtcDateTime >= startDate && consumeResult.Message.Timestamp.UtcDateTime <= endDate)
                        {
                            messages.Add(consumeResult.Message.Value);
                        }

                        if (consumeResult.Offset >= endOffset.Offset && endOffset.Offset != Offset.End)
                            break;
                    }
                }
            }
            return messages;
        }
    }
}