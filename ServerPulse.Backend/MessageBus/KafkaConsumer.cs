﻿using Confluent.Kafka;
using System.Text.Json;

namespace TestKafka.Consumer.Services
{
    public class KafkaConsumer<T> : IMessageConsumer<T>
    {
        private readonly IAdminClient adminClient;
        private readonly ConsumerConfig consumerConfig;
        private readonly ConsumerBuilder<string, string> consumerBuilder;

        public event EventHandler<JsonException> OnJsonConvertException = default!;

        public KafkaConsumer(IAdminClient adminClient, ConsumerConfig consumerConfig)
        {
            this.adminClient = adminClient;
            this.consumerConfig = consumerConfig;
            consumerBuilder = new ConsumerBuilder<string, string>(consumerConfig);
        }

        private async Task<T?> ReadLastTopicMessageAsync(string topicName, int timeoutInMilliseconds = 2000, CancellationToken cancellationToken = default!)
        {
            var val = await Task.Run(() => ReadLastTopicMessage(topicName, timeoutInMilliseconds, cancellationToken));
            return val;
        }
        public T? ReadLastTopicMessage(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            using (var consumer = GetConsumer())
            {
                consumer.Subscribe(topicName);
                var topicMetadata = adminClient.GetMetadata(topicName, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
                var partitionMetadatas = topicMetadata.Topics.First(x => x.Topic == topicName).Partitions;
                var topicPartitions = partitionMetadatas.Select(p => new TopicPartition(topicName, p.PartitionId)).ToList();

                ConsumeResult<string, string>? latestConsumeResult = null;

                foreach (var partition in topicPartitions)
                {
                    var watermarkOffsets = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
                    var lastOffset = watermarkOffsets.High - 1;

                    if (lastOffset < 0)
                        continue; // Skip empty partitions

                    consumer.Assign(new List<TopicPartitionOffset> { new TopicPartitionOffset(partition, lastOffset) });
                    consumer.Seek(new TopicPartitionOffset(partition, lastOffset));

                    var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(timeoutInMilliseconds));
                    if (consumeResult == null || string.IsNullOrEmpty(consumeResult.Message.Value))
                        continue;

                    if (latestConsumeResult == null || consumeResult.Message.Timestamp.UtcDateTime > latestConsumeResult.Message.Timestamp.UtcDateTime)
                    {
                        latestConsumeResult = consumeResult;
                    }

                }

                if (latestConsumeResult != null)
                {
                    try
                    {
                        var deserializedObject = JsonSerializer.Deserialize<T>(latestConsumeResult.Message.Value);
                        return deserializedObject;
                    }
                    catch (JsonException ex)
                    {
                        OnJsonConvertException?.Invoke(this, ex);
                    }
                }

                return default;
            }
        }
        public async Task<List<T>> ReadMessagesInDateRangeAsync(string topicName, DateTime startDate, DateTime endDate, int timeoutInMilliseconds = 2000, CancellationToken cancellationToken = default!)
        {
            var val = await Task.Run(() => ReadMessagesInDateRange(topicName, startDate, endDate, timeoutInMilliseconds, cancellationToken));
            return val;
        }
        private List<T> ReadMessagesInDateRange(string topicName, DateTime startDate, DateTime endDate, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            if (startDate.ToUniversalTime() >= DateTime.UtcNow || startDate.ToUniversalTime() >= endDate.ToUniversalTime())
            {
                throw new Exception("Invalid Start Date! Must be less than now (UTC) and End Date!");
            }

            var messages = new List<T>();
            using (var consumer = GetConsumer())
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
                        try
                        {
                            var consumeResult = consumer.Consume(timeoutInMilliseconds);
                            if (consumeResult == null)
                                break;
                            if (string.IsNullOrEmpty(consumeResult.Message.Value))
                                continue;

                            if (consumeResult.Message.Timestamp.UtcDateTime >= startDate && consumeResult.Message.Timestamp.UtcDateTime <= endDate)
                            {
                                var ev = JsonSerializer.Deserialize<T>(consumeResult.Message.Value);
                                messages.Add(ev);
                            }

                            if (consumeResult.Offset >= endOffset.Offset && endOffset.Offset != Offset.End)
                                break;
                        }
                        catch (JsonException ex)
                        {
                            OnJsonConvertException?.Invoke(this, ex);
                        }
                    }
                }
            }
            return messages;
        }
        private IConsumer<string, string> GetConsumer()
        {
            return consumerBuilder.Build();
        }
    }
}