﻿using Confluent.Kafka;
using MessageBus.Interfaces;
using System.Collections.Concurrent;
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

        #region IMessageConsumer Members

        public async IAsyncEnumerable<ConsumeResponse> ConsumeAsync(string topic, int timeoutInMilliseconds, Offset consumeFrom, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var consumer = consumerFactory.CreateConsumer();
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
        public async Task<ConsumeResponse?> ReadLastTopicMessageAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            var partitionMetadata = GetPartitionMetadata(topicName, timeoutInMilliseconds);
            if (partitionMetadata == null)
            {
                return null;
            }

            var latestMessages = await Task.WhenAll(partitionMetadata.Select(partition =>
                Task.Run(() => ReadPartitionLatestMessage(topicName, partition.PartitionId, timeoutInMilliseconds, cancellationToken))
            ));

            var latestMessage = latestMessages
                .Where(result => result != null)
                .OrderByDescending(result => result?.Message.Timestamp.UtcDateTime)
                .FirstOrDefault();

            return latestMessage == null ? null : new ConsumeResponse(latestMessage.Message.Value, latestMessage.Message.Timestamp.UtcDateTime);
        }
        private ConsumeResult<string, string>? ReadPartitionLatestMessage(string topicName, int partitionId, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            using var consumer = consumerFactory.CreateConsumer();
            var partition = new TopicPartition(topicName, new Partition(partitionId));
            var highOffset = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromMilliseconds(timeoutInMilliseconds)).High;

            if (highOffset.Value == 0)
            {
                return null; // No messages in this partition
            }

            var latestOffset = new TopicPartitionOffset(partition, highOffset - 1);
            consumer.Assign(latestOffset);

            var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(timeoutInMilliseconds));
            return consumeResult?.Message?.Value == null || string.IsNullOrEmpty(consumeResult.Message.Value) ? null : consumeResult;
        }
        public Task<List<ConsumeResponse>> ReadMessagesInDateRangeAsync(MessageInRangeQueryOptions options, CancellationToken cancellationToken)
        {
            var startDate = options.From.ToUniversalTime();
            var endDate = options.To.ToUniversalTime();

            if (startDate > DateTime.UtcNow || startDate > endDate)
            {
                throw new ArgumentException("Invalid Start Date! Must be less or equal than now (UTC) and End Date!");
            }

            var threadResults = new List<List<ConsumeResponse>>();

            List<TopicPartitionOffset> startOffsets;
            List<TopicPartitionOffset> endOffsets;

            using (var consumer = consumerFactory.CreateConsumer())
            {
                var partitions = GetPartitionIdsForTopic(options.TopicName, options.TimeoutInMilliseconds);

                startOffsets = consumer.OffsetsForTimes(partitions.Select(p =>
                    new TopicPartitionTimestamp(new TopicPartition(options.TopicName, p), new Timestamp(startDate))),
                    TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds)).ToList();

                endOffsets = consumer.OffsetsForTimes(partitions.Select(p =>
                    new TopicPartitionTimestamp(new TopicPartition(options.TopicName, p), new Timestamp(endDate))),
                    TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds)).ToList();
            }

            Parallel.ForEach(startOffsets, new ParallelOptions { CancellationToken = cancellationToken }, startOffset =>
            {
                var endOffset = endOffsets.First(e => e.TopicPartition == startOffset.TopicPartition);
                var partitionMessages = new List<ConsumeResponse>();
                var currentTopicOffset = startOffset;
                ConsumeResult<string, string>? consumeResult = null;

                while (!cancellationToken.IsCancellationRequested)
                {
                    using (var consumer = consumerFactory.CreateConsumer())
                    {
                        consumer.Assign(currentTopicOffset);
                        consumeResult = consumer.Consume(options.TimeoutInMilliseconds);
                    }

                    if (consumeResult?.Message?.Value == null)
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

                    currentTopicOffset = new TopicPartitionOffset(consumeResult.TopicPartition, consumeResult.Offset + 1);
                }

                lock (threadResults)
                {
                    threadResults.Add(partitionMessages);
                }
            });

            var result = threadResults.SelectMany(x => x)
                                       .OrderByDescending(x => x.CreationTimeUTC)
                                       .ToList();

            return Task.FromResult(result);
        }
        public Task<int> GetAmountTopicMessagesAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            var partitions = GetTopicPartitions(topicName, timeoutInMilliseconds);
            long totalMessages = 0;

            using var consumer = consumerFactory.CreateConsumer();
            foreach (var partition in partitions)
            {
                var watermarks = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
                totalMessages += watermarks.High - watermarks.Low;
            }

            return Task.FromResult((int)totalMessages);
        }
        public Task<Dictionary<DateTime, int>> GetMessageAmountPerTimespanAsync(MessageInRangeQueryOptions options, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            var fromDate = options.From.ToUniversalTime();
            var toDate = options.To.ToUniversalTime();

            using var consumer = consumerFactory.CreateConsumer();

            var topicMetadata = adminClient.GetMetadata(options.TopicName, TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds));
            var partitions = topicMetadata.Topics
                .First(x => x.Topic == options.TopicName)
                .Partitions.Select(p => new TopicPartition(options.TopicName, p.PartitionId)).ToList();

            var threadResults = new List<Dictionary<DateTime, int>>();

            Parallel.ForEach(partitions, new ParallelOptions { CancellationToken = cancellationToken }, partition =>
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
            });

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

            return Task.FromResult(finalResult);
        }
        public Task<List<ConsumeResponse>> ReadSomeMessagesAsync(ReadSomeMessagesOptions options, CancellationToken cancellationToken)
        {
            var startDate = options.StartDate.ToUniversalTime();
            var threadResults = new ConcurrentBag<List<ConsumeResponse>>();
            List<TopicPartitionOffset> startOffsets;

            using (var consumer = consumerFactory.CreateConsumer())
            {
                var partitions = GetPartitionIdsForTopic(options.TopicName, options.TimeoutInMilliseconds);

                startOffsets = consumer.OffsetsForTimes(
                    partitions.Select(p => new TopicPartitionTimestamp(new TopicPartition(options.TopicName, p), new Timestamp(startDate))),
                    TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds)).ToList();
            }

            Parallel.ForEach(startOffsets, new ParallelOptions { CancellationToken = cancellationToken }, startOffset =>
            {
                var partitionMessages = new List<ConsumeResponse>();
                int totalMessagesRead = 0;
                Offset lowWatermark;
                Offset highWatermark;

                using (var consumer = consumerFactory.CreateConsumer())
                {
                    var watermarks = consumer.QueryWatermarkOffsets(startOffset.TopicPartition, TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds));
                    lowWatermark = watermarks.Low;
                    highWatermark = watermarks.High;

                    if (startOffset.Offset == Offset.End || startOffset.Offset > highWatermark)
                    {
                        startOffset = new TopicPartitionOffset(startOffset.TopicPartition, highWatermark - 1);
                    }
                    else if (startOffset.Offset < lowWatermark)
                    {
                        startOffset = new TopicPartitionOffset(startOffset.TopicPartition, lowWatermark);
                    }
                }

                var currentTopicOffset = startOffset;

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (totalMessagesRead >= options.NumberOfMessages)
                        break;

                    ConsumeResult<string, string>? consumeResult;
                    using (var consumer = consumerFactory.CreateConsumer())
                    {
                        consumer.Assign(currentTopicOffset);
                        consumeResult = consumer.Consume(options.TimeoutInMilliseconds);
                    }

                    if (consumeResult?.Message?.Value == null)
                        break;

                    var messageTimestamp = consumeResult.Message.Timestamp.UtcDateTime;

                    if ((messageTimestamp >= startDate && options.ReadNew) ||
                        (messageTimestamp <= startDate && !options.ReadNew))
                    {
                        if (totalMessagesRead++ < options.NumberOfMessages)
                        {
                            partitionMessages.Add(new ConsumeResponse(consumeResult.Message.Value, messageTimestamp));
                        }
                        else
                        {
                            break;
                        }
                    }

                    currentTopicOffset = GetNextOffset(consumeResult, options, lowWatermark, highWatermark);
                    if (currentTopicOffset == null)
                        break;
                }

                threadResults.Add(partitionMessages);
            });

            var result = threadResults.SelectMany(x => x).ToList();

            if (options.ReadNew)
            {
                result = result.OrderBy(x => x.CreationTimeUTC).ToList();
            }
            else
            {
                result = result.OrderByDescending(x => x.CreationTimeUTC).ToList();
            }

            result = result.Take(options.NumberOfMessages).ToList();
            return Task.FromResult(result);
        }

        #endregion

        #region Private Helpers

        private TopicPartitionOffset? GetNextOffset(ConsumeResult<string, string> consumeResult, ReadSomeMessagesOptions options, Offset lowWatermark, Offset highWatermark)
        {
            var currentOffset = consumeResult.Offset;
            if (!options.ReadNew)
            {
                var nextOffset = new Offset(currentOffset - 1);
                if (nextOffset < lowWatermark)
                    return null;

                return new TopicPartitionOffset(consumeResult.TopicPartition, nextOffset);
            }
            else
            {
                if (currentOffset >= highWatermark)
                    return null;

                return new TopicPartitionOffset(consumeResult.TopicPartition, currentOffset + 1);
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
        private List<int> GetPartitionIdsForTopic(string topicName, int timeoutInMilliseconds)
        {
            var topicMetadata = adminClient.GetMetadata(topicName, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
            return topicMetadata.Topics
                                .First(x => x.Topic == topicName)
                                .Partitions
                                .Select(p => p.PartitionId)
                                .ToList();
        }
        private IEnumerable<TopicPartition> GetTopicPartitions(string topicName, int timeoutInMilliseconds)
        {
            var metadata = adminClient.GetMetadata(topicName, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
            return metadata.Topics.First(x => x.Topic == topicName).Partitions.Select(p => new TopicPartition(topicName, p.PartitionId));
        }
        private IEnumerable<PartitionMetadata?>? GetPartitionMetadata(string topicName, int timeoutInMilliseconds)
        {
            return adminClient.GetMetadata(topicName, TimeSpan.FromMilliseconds(timeoutInMilliseconds))
                .Topics.FirstOrDefault(x => x.Topic == topicName)?.Partitions;
        }
        private bool IsValidMessage(ConsumeResult<string, string> consumeResult)
        {
            return consumeResult?.Message != null && !consumeResult.IsPartitionEOF && !string.IsNullOrEmpty(consumeResult.Message.Value);
        }

        #endregion
    }
}