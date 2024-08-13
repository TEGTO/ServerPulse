using Confluent.Kafka;
using MessageBus.Interfaces;
using System.Runtime.CompilerServices;

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

        public async IAsyncEnumerable<ConsumeResponse> ConsumeAsync(string topic, int timeoutInMilliseconds, Offset consumeFrom, [EnumeratorCancellation] CancellationToken cancellationToken)
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
                        yield return new ConsumeResponse(consumeResult.Message.Value, consumeResult.Message.Timestamp.UtcDateTime);
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
        public async Task<ConsumeResponse?> ReadLastTopicMessageAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            var val = await Task.Run(() =>
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

                if (latestMessage == null)
                {
                    return null;
                }

                return new ConsumeResponse(latestMessage.Message.Value, latestMessage.Message.Timestamp.UtcDateTime);
            });

            return val;
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
        public async Task<List<ConsumeResponse>> ReadMessagesInDateRangeAsync(MessageInRangeQueryOptions options, CancellationToken cancellationToken)
        {
            var startDate = options.From.ToUniversalTime();
            var endDate = options.To.ToUniversalTime();

            if (startDate > DateTime.UtcNow || startDate > endDate)
            {
                throw new ArgumentException("Invalid Start Date! Must be less or equal than now (UTC) and End Date!");
            }

            var threadResults = new List<List<ConsumeResponse>>();

            using (var consumer = consumerFactory.CreateConsumer())
            {
                var topicMetadata = adminClient.GetMetadata(options.TopicName, TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds));
                var partitions = topicMetadata.Topics.First(x => x.Topic == options.TopicName).Partitions.Select(p => p.PartitionId).ToList();

                var startOffsets = consumer.OffsetsForTimes(partitions.Select(p =>
                    new TopicPartitionTimestamp(new TopicPartition(options.TopicName, p), new Timestamp(startDate))),
                    TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds)).ToList();

                var endOffsets = consumer.OffsetsForTimes(partitions.Select(p =>
                    new TopicPartitionTimestamp(new TopicPartition(options.TopicName, p), new Timestamp(endDate))),
                    TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds)).ToList();

                consumer.Assign(startOffsets);

                var consumeTasks = startOffsets.Select(startOffset =>
                {
                    var endOffset = endOffsets.First(e => e.TopicPartition == startOffset.TopicPartition);
                    var partitionMessages = new List<ConsumeResponse>();

                    return Task.Run(() =>
                    {
                        consumer.Seek(startOffset);

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var consumeResult = consumer.Consume(options.TimeoutInMilliseconds);
                            if (consumeResult == null || consumeResult.Message == null || string.IsNullOrEmpty(consumeResult.Message.Value))
                                break;

                            var messageTimestamp = consumeResult.Message.Timestamp.UtcDateTime;

                            if (messageTimestamp > endDate)
                                break;

                            if (messageTimestamp >= startDate && messageTimestamp <= endDate)
                            {
                                partitionMessages.Add(new ConsumeResponse(consumeResult.Message.Value, messageTimestamp));
                            }

                            if (consumeResult.Offset >= endOffset.Offset && endOffset.Offset != Offset.End)
                                break;
                        }

                        lock (threadResults)
                        {
                            threadResults.Add(partitionMessages);
                        }

                    }, cancellationToken);

                }).ToList();

                await Task.WhenAll(consumeTasks);
            }

            return threadResults.SelectMany(x => x)
                                .OrderByDescending(x => x.CreationTimeUTC)
                                .ToList();
        }
        private bool IsValidMessage(ConsumeResult<string, string> consumeResult)
        {
            return
                 consumeResult?.Message != null
                && !consumeResult.IsPartitionEOF
                && !string.IsNullOrEmpty(consumeResult.Message.Value);
        }
        public async Task<int> GetAmountTopicMessagesAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            var val = await Task.Run(() =>
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
            });
            return val;
        }
        public async Task<Dictionary<DateTime, int>> GetMessageAmountPerTimespanAsync(MessageInRangeQueryOptions options, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            var fromDate = options.From.ToUniversalTime();
            var toDate = options.To.ToUniversalTime();

            using (var consumer = consumerFactory.CreateConsumer())
            {
                var topicMetadata = adminClient.GetMetadata(options.TopicName, TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds));
                var partitions = topicMetadata.Topics
                    .First(x => x.Topic == options.TopicName)
                    .Partitions.Select(p => new TopicPartition(options.TopicName, p.PartitionId)).ToList();

                var threadResults = new List<Dictionary<DateTime, int>>();

                var tasks = partitions.Select(partition => Task.Run(() =>
                {
                    var messagesPerTimespan = new Dictionary<DateTime, int>();

                    var watermarks = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds));
                    var endOffset = watermarks.High;

                    var fromOffsets = consumer.OffsetsForTimes(new[]
                    {
                        new TopicPartitionTimestamp(partition, new Timestamp(fromDate)),
                    }, TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds));

                    var toOffsets = consumer.OffsetsForTimes(new[]
                    {
                        new TopicPartitionTimestamp(partition, new Timestamp(toDate)),
                    }, TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds));

                    var fromOffset = fromOffsets.FirstOrDefault();
                    var toOffset = toOffsets.FirstOrDefault();

                    if (fromOffset != null && fromOffset.Offset != Offset.End)
                    {
                        var currentOffset = fromOffset.Offset;
                        var lastOffset = toOffset == null || toOffset.Offset == Offset.End ? endOffset : toOffset.Offset;
                        var currentDate = fromDate;

                        while (currentOffset < lastOffset && !cancellationToken.IsCancellationRequested)
                        {
                            var nextDate = currentDate.Add(timeSpan);
                            var nextOffsets = consumer.OffsetsForTimes(new[]
                            {
                                new TopicPartitionTimestamp(partition, new Timestamp(nextDate))
                            }, TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds));
                            var nextOffset = nextOffsets.FirstOrDefault();
                            if (nextOffset != null && nextOffset.Offset != Offset.End && nextOffset.Offset != Offset.Unset && nextOffset.Offset < lastOffset)
                            {
                                messagesPerTimespan[currentDate] = (int)(nextOffset.Offset - currentOffset);
                                currentOffset = nextOffset.Offset;
                                currentDate = nextDate;
                            }
                            else
                            {
                                messagesPerTimespan[currentDate] = (int)(lastOffset - currentOffset);
                                break;
                            }
                        }
                    }
                    else
                    {
                        messagesPerTimespan[fromDate] = 0;
                    }

                    lock (threadResults)
                    {
                        threadResults.Add(messagesPerTimespan);
                    }
                }, cancellationToken)).ToArray();

                await Task.WhenAll(tasks);

                var finalResult = new Dictionary<DateTime, int>();
                foreach (var threadResult in threadResults)
                {
                    foreach (var kvp in threadResult)
                    {
                        if (finalResult.ContainsKey(kvp.Key))
                        {
                            finalResult[kvp.Key] += kvp.Value;
                        }
                        else
                        {
                            finalResult[kvp.Key] = kvp.Value;
                        }
                    }
                }
                return finalResult;
            }
        }
        public async Task<List<ConsumeResponse>> ReadSomeMessagesAsync(ReadSomeMessagesOptions options, CancellationToken cancellationToken)
        {
            var startDate = options.StartDate.ToUniversalTime();
            var partitionMessagesList = new List<List<ConsumeResponse>>();

            using (var consumer = consumerFactory.CreateConsumer())
            {
                var topicMetadata = adminClient.GetMetadata(options.TopicName, TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds));
                var partitions = topicMetadata.Topics.First(x => x.Topic == options.TopicName).Partitions.Select(p => p.PartitionId).ToList();

                var startOffsets = consumer.OffsetsForTimes(
                    partitions.Select(p => new TopicPartitionTimestamp(new TopicPartition(options.TopicName, p), new Timestamp(startDate))),
                    TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds)).ToList();

                consumer.Assign(startOffsets);

                int totalMessagesRead = 0;

                var consumeTasks = startOffsets.Select(startOffset =>
                {
                    var partitionMessages = new List<ConsumeResponse>();

                    return Task.Run(() =>
                    {
                        var watermarks = consumer.QueryWatermarkOffsets(startOffset.TopicPartition, TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds));
                        var lowWatermark = watermarks.Low;
                        var highWatermark = watermarks.High;

                        if (startOffset.Offset == Offset.End)
                        {
                            startOffset = new TopicPartitionOffset(startOffset.TopicPartition, highWatermark - 1);
                        }

                        consumer.Seek(startOffset);

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            if (totalMessagesRead >= options.NumberOfMessages)
                                break;

                            var consumeResult = consumer.Consume(options.TimeoutInMilliseconds);
                            if (consumeResult == null || consumeResult.Message == null || string.IsNullOrEmpty(consumeResult.Message.Value))
                                break;

                            var messageTimestamp = consumeResult.Message.Timestamp.UtcDateTime;

                            if ((messageTimestamp >= startDate && options.ReadNew) ||
                                (messageTimestamp <= startDate && !options.ReadNew))
                            {
                                if (totalMessagesRead < options.NumberOfMessages)
                                {
                                    partitionMessages.Add(new ConsumeResponse(consumeResult.Message.Value, messageTimestamp));
                                    Interlocked.Increment(ref totalMessagesRead);
                                }
                            }

                            if (!options.ReadNew)
                            {
                                var currentOffset = consumeResult.Offset;
                                var nextOffset = new Offset(currentOffset - 1);

                                if (nextOffset < lowWatermark)
                                    break;

                                consumer.Seek(new TopicPartitionOffset(consumeResult.TopicPartition, nextOffset));
                            }
                        }

                        lock (partitionMessagesList)
                        {
                            partitionMessagesList.Add(partitionMessages);
                        }

                    }, cancellationToken);

                }).ToList();

                await Task.WhenAll(consumeTasks);
            }

            return partitionMessagesList.SelectMany(x => x)
                                        .OrderByDescending(x => x.CreationTimeUTC)
                                        .ToList();
        }
    }
}