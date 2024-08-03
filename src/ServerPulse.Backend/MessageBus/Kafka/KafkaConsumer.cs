using Confluent.Kafka;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using TestKafka.Consumer.Services;

namespace MessageBus.Kafka
{
    public class KafkaConsumer : IMessageConsumer
    {
        private readonly IAdminClient adminClient;
        private readonly IKafkaConsumerFactory consumerFactory;

        public KafkaConsumer(IAdminClient adminClient, IKafkaConsumerFactory consumerFactory)
        {
            this.adminClient = adminClient;
            this.consumerFactory = consumerFactory;
        }

        public async IAsyncEnumerable<string> ConsumeAsync(string topic, int timeoutInMilliseconds, Offset consumeFrom, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using (var consumer = consumerFactory.CreateConsumer())
            {
                var partitions = GetTopicPartitionOffsets(topic, timeoutInMilliseconds, consumeFrom);
                consumer.Assign(partitions);
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(timeoutInMilliseconds));
                    if (IsValidMessage(consumeResult))
                    {
                        yield return consumeResult.Message.Value;
                    }
                    await Task.Yield();
                }
            }
        }
        private List<TopicPartitionOffset> GetTopicPartitionOffsets(string topic, int timeoutInMilliseconds, Offset offset)
        {
            var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(timeoutInMilliseconds));
            var partitions = new List<TopicPartitionOffset>();
            foreach (var partition in metadata.Topics[0].Partitions)
            {
                partitions.Add(new TopicPartitionOffset(topic, partition.PartitionId, offset));
            }
            return partitions;
        }
        private bool IsValidMessage(ConsumeResult<string, string> consumeResult)
        {
            return
                 consumeResult?.Message != null
                && !consumeResult.IsPartitionEOF
                && !string.IsNullOrEmpty(consumeResult.Message.Value);
        }
        public async Task<string?> ReadLastTopicMessageAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            var val = await Task.Run(() => ReadLastTopicMessage(topicName, timeoutInMilliseconds, cancellationToken));
            return val;
        }
        public string? ReadLastTopicMessage(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            var topicMetadata = adminClient.GetMetadata(topicName, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
            var partitionMetadatas = topicMetadata.Topics.FirstOrDefault(x => x.Topic == topicName)?.Partitions;

            if (partitionMetadatas == null)
            {
                return null;
            }

            var tasks = partitionMetadatas.Select(partition =>
                Task.Run(() => ReadPartitionLatestMessage(topicName, partition.PartitionId, timeoutInMilliseconds, cancellationToken))
            ).ToList();

            Task.WaitAll(tasks.ToArray(), cancellationToken);

            var latestMessage = tasks
                .Select(task => task.Result)
                .Where(result => result != null)
                .OrderByDescending(result => result.Message.Timestamp.UtcDateTime)
                .FirstOrDefault();

            return latestMessage?.Message.Value;
        }
        private ConsumeResult<string, string>? ReadPartitionLatestMessage(string topicName, int partitionId, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            using (var consumer = consumerFactory.CreateConsumer())
            {
                var partition = new TopicPartition(topicName, new Partition(partitionId));
                var watermarkOffsets = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
                if (watermarkOffsets.High.Value == 0)
                {
                    return null; // No messages in this partition
                }
                var topicPartitionOffset = new TopicPartitionOffset(partition, new Offset(watermarkOffsets.High.Value - 1));
                consumer.Assign(topicPartitionOffset);

                var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(timeoutInMilliseconds));
                if (consumeResult == null || string.IsNullOrEmpty(consumeResult.Message.Value))
                    return null;

                return consumeResult;
            }
        }
        public async Task<List<string>> ReadMessagesInDateRangeAsync(string topicName, DateTime startDate, DateTime endDate, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            if (startDate.ToUniversalTime() >= DateTime.UtcNow || startDate.ToUniversalTime() >= endDate.ToUniversalTime())
            {
                throw new Exception("Invalid Start Date! Must be less than now (UTC) and End Date!");
            }

            var messages = new ConcurrentBag<string>();
            var tasks = new List<Task>();

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
                    var endOffset = endOffsets.First(e => e.TopicPartition == startOffset.TopicPartition);

                    tasks.Add(Task.Run(() =>
                    {
                        consumer.Seek(startOffset);

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
                    }, cancellationToken));
                }

                await Task.WhenAll(tasks);
            }

            return messages.ToList();
        }

        public async Task<int> GetAmountTopicMessagesAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            var val = await Task.Run(() => GetAmountTopicMessages(topicName, timeoutInMilliseconds, cancellationToken));
            return val;
        }
        private int GetAmountTopicMessages(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            using (var consumer = consumerFactory.CreateConsumer())
            {
                var topicMetadata = adminClient.GetMetadata(topicName, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
                var partitionMetadatas = topicMetadata.Topics.First(x => x.Topic == topicName).Partitions;
                var partitions = partitionMetadatas.Select(p => new TopicPartition(topicName, p.PartitionId)).ToList();

                long total = 0;
                foreach (var partition in partitions)
                {
                    WatermarkOffsets watermarkOffsets = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
                    total += watermarkOffsets.High - watermarkOffsets.Low;
                }
                return (int)total;
            }
        }
    }
}