using Confluent.Kafka;
using MessageBus.Interfaces;
using MessageBus.Models;
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
            await WaitForTopicAsync(topic, timeoutInMilliseconds, cancellationToken).ConfigureAwait(false);

            consumeFrom = await AdjustOffsetAsync(topic, timeoutInMilliseconds, consumeFrom, cancellationToken);

            var topicPartitionOffsets = GetTopicPartitionMetadata(topic, timeoutInMilliseconds)
                .Select(x => new TopicPartitionOffset(topic, x.PartitionId, consumeFrom));

            using var consumer = consumerFactory.CreateConsumer();
            consumer.Assign(topicPartitionOffsets);

            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(timeoutInMilliseconds));

                if (!IsConsumeResultValid(consumeResult))
                {
                    await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                yield return new ConsumeResponse(consumeResult.Message.Value, consumeResult.Message.Timestamp.UtcDateTime);
            }
        }

        public async Task<ConsumeResponse?> GetLastTopicMessageAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            var topicPartitions = GetTopicPartitionMetadata(topicName, timeoutInMilliseconds)
                .Select(p => new TopicPartition(topicName, p.PartitionId));

            if (!topicPartitions.Any())
            {
                return null;
            }

            var latestMessages = await Task.WhenAll(topicPartitions.Select(partition =>
                Task.Run(() => ReadPartitionLatestMessage(partition, timeoutInMilliseconds))
            )).ConfigureAwait(false);

            var latestMessage = latestMessages
                .Where(result => result != null)
                .OrderByDescending(result => result?.Message.Timestamp.UtcDateTime)
                .FirstOrDefault();

            if (latestMessage == null)
            {
                return null;
            }

            return new ConsumeResponse(latestMessage.Message.Value, latestMessage.Message.Timestamp.UtcDateTime);
        }

        private ConsumeResult<string, string>? ReadPartitionLatestMessage(TopicPartition partition, int timeoutInMilliseconds)
        {
            using var consumer = consumerFactory.CreateConsumer();
            var endOffset = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromMilliseconds(timeoutInMilliseconds)).High;

            if (endOffset.Value == 0)
            {
                return null; // No messages in this partition
            }

            var latestOffset = new TopicPartitionOffset(partition, endOffset - 1);
            consumer.Assign(latestOffset);

            var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(timeoutInMilliseconds));

            if (consumeResult?.Message?.Value == null || string.IsNullOrWhiteSpace(consumeResult.Message.Value))
            {
                return null;
            }

            return consumeResult;
        }

        public async Task<IEnumerable<ConsumeResponse>> GetMessagesInDateRangeAsync(GetMessageInDateRangeOptions options, CancellationToken cancellationToken)
        {
            var startDate = options.From.ToUniversalTime();
            var endDate = options.To.ToUniversalTime();

            if (startDate > DateTime.UtcNow || startDate > endDate)
            {
                throw new ArgumentException($"Invalid Start Date ({startDate})! Must be less or equal than current UTC ({DateTime.UtcNow}) and End Date ({endDate})!");
            }

            List<TopicPartitionOffset> startOffsets;
            List<TopicPartitionOffset> endOffsets;

            using (var consumer = consumerFactory.CreateConsumer())
            {
                var topicPartitionIds = GetTopicPartitionMetadata(options.TopicName, options.TimeoutInMilliseconds).Select(p => p.PartitionId);

                startOffsets = consumer.OffsetsForTimes(topicPartitionIds.Select(p =>
                    new TopicPartitionTimestamp(new TopicPartition(options.TopicName, p), new Timestamp(startDate))),
                    TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds)).ToList();

                endOffsets = consumer.OffsetsForTimes(topicPartitionIds.Select(p =>
                    new TopicPartitionTimestamp(new TopicPartition(options.TopicName, p), new Timestamp(endDate))),
                    TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds)).ToList();
            }

            var partitionTasks = startOffsets.Select(startOffset =>
            {
                var endOffset = endOffsets.First(e => e.TopicPartition == startOffset.TopicPartition);
                return Task.Run(() => GetPartitionMessagesInDateRange(startOffset, endOffset, startDate, endDate, options.TimeoutInMilliseconds, cancellationToken));
            });

            var partitionMessages = await Task.WhenAll(partitionTasks).ConfigureAwait(false);

            var result = partitionMessages.SelectMany(x => x).OrderByDescending(x => x.CreationTimeUTC);

            return result;
        }

        private IEnumerable<ConsumeResponse> GetPartitionMessagesInDateRange(
            TopicPartitionOffset startOffset,
            TopicPartitionOffset endOffset,
            DateTime startDate,
            DateTime endDate,
            int timeoutInMilliseconds,
            CancellationToken cancellationToken)
        {
            var partitionMessages = new List<ConsumeResponse>();
            var currentTopicOffset = startOffset;

            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? consumeResult;

                using (var consumer = consumerFactory.CreateConsumer())
                {
                    consumer.Assign(currentTopicOffset);
                    consumeResult = consumer.Consume(timeoutInMilliseconds);
                }

                if (consumeResult?.Message?.Value == null)
                {
                    break;
                }

                var messageTimestamp = consumeResult.Message.Timestamp.UtcDateTime;

                if (messageTimestamp > endDate)
                {
                    break;
                }

                if (messageTimestamp >= startDate && messageTimestamp <= endDate)
                {
                    partitionMessages.Add(new ConsumeResponse(consumeResult.Message.Value, messageTimestamp));
                }

                if (consumeResult.Offset >= endOffset.Offset && endOffset.Offset != Offset.End)
                {
                    break;
                }

                currentTopicOffset = new TopicPartitionOffset(consumeResult.TopicPartition, consumeResult.Offset + 1);
            }

            return partitionMessages;
        }

        public Task<int> GetTopicMessageAmountAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            var topicPartitions = GetTopicPartitionMetadata(topicName, timeoutInMilliseconds).Select(p => new TopicPartition(topicName, p.PartitionId));
            long totalMessages = 0;

            using var consumer = consumerFactory.CreateConsumer();
            foreach (var partition in topicPartitions)
            {
                var watermarks = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
                totalMessages += watermarks.High - watermarks.Low;
            }

            return Task.FromResult((int)totalMessages);
        }

        public async Task<Dictionary<DateTime, int>> GetTopicMessageAmountPerTimespanAsync(GetMessageInDateRangeOptions options, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            var fromDate = options.From.ToUniversalTime();
            var toDate = options.To.ToUniversalTime();

            if (fromDate > DateTime.UtcNow || fromDate > toDate)
            {
                throw new ArgumentException($"Invalid From Date ({fromDate})! Must be less or equal than UTC ({DateTime.UtcNow}) and To Date ({toDate})!");
            }

            using var consumer = consumerFactory.CreateConsumer();

            var topicPartitions = GetTopicPartitionMetadata(options.TopicName, options.TimeoutInMilliseconds)
                .Select(p => new TopicPartition(options.TopicName, p.PartitionId));

            var partitionTasks = topicPartitions.Select(partition =>
                Task.Run(() => GetPartitionMessageAmountPerTimespan(partition, consumer, fromDate, toDate, timeSpan,
                                                options.TimeoutInMilliseconds, cancellationToken), cancellationToken));

            var partitionMessagePerTimeSPanAmounts = await Task.WhenAll(partitionTasks).ConfigureAwait(false);

            var result = new Dictionary<DateTime, int>();
            foreach (var amountPerTimespan in partitionMessagePerTimeSPanAmounts)
            {
                foreach (var kvp in amountPerTimespan)
                {
                    if (result.ContainsKey(kvp.Key))
                    {
                        result[kvp.Key] += kvp.Value;
                    }
                    else
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }
            }

            return result;
        }

        private static Dictionary<DateTime, int> GetPartitionMessageAmountPerTimespan(
            TopicPartition partition,
            IConsumer<string, string> consumer,
            DateTime fromDate,
            DateTime toDate,
            TimeSpan timeSpan,
            int timeoutInMilliseconds,
            CancellationToken cancellationToken)
        {
            var messageAmountPerTimespan = new Dictionary<DateTime, int>();

            var watermarks = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromMilliseconds(timeoutInMilliseconds));

            var endOffset = watermarks.High;

            var fromPartitionOffsets = consumer.OffsetsForTimes(
            [
                new TopicPartitionTimestamp(partition, new Timestamp(fromDate)),
            ], TimeSpan.FromMilliseconds(timeoutInMilliseconds));

            var toPartitionOffsets = consumer.OffsetsForTimes(
            [
                new TopicPartitionTimestamp(partition, new Timestamp(toDate)),
            ], TimeSpan.FromMilliseconds(timeoutInMilliseconds));

            var fromPartitionOffset = fromPartitionOffsets.FirstOrDefault();
            var toPartitionOffset = toPartitionOffsets.FirstOrDefault();

            if (fromPartitionOffset != null && fromPartitionOffset.Offset != Offset.End)
            {
                var currentOffset = fromPartitionOffset.Offset;
                var lastOffset = toPartitionOffset == null || toPartitionOffset.Offset == Offset.End ? endOffset : toPartitionOffset.Offset;
                var currentDate = fromDate;

                while (currentOffset < lastOffset && !cancellationToken.IsCancellationRequested)
                {
                    var nextDate = currentDate.Add(timeSpan);

                    var nextPartitionOffsets = consumer.OffsetsForTimes(
                    [
                        new TopicPartitionTimestamp(partition, new Timestamp(nextDate))
                    ], TimeSpan.FromMilliseconds(timeoutInMilliseconds));

                    var nextPartitionOffset = nextPartitionOffsets.FirstOrDefault();

                    if (IsNextPartitionOffSetValid(nextPartitionOffset, lastOffset))
                    {
                        messageAmountPerTimespan[currentDate] = (int)(nextPartitionOffset!.Offset - currentOffset);
                        currentOffset = nextPartitionOffset.Offset;
                        currentDate = nextDate;
                    }
                    else
                    {
                        messageAmountPerTimespan[currentDate] = (int)(lastOffset - currentOffset);
                        break;
                    }
                }
            }
            else
            {
                messageAmountPerTimespan[fromDate] = 0;
            }

            return messageAmountPerTimespan;
        }

        public async Task<IEnumerable<ConsumeResponse>> GetSomeMessagesStartFromDateAsync(GetSomeMessagesFromDateOptions options, CancellationToken cancellationToken)
        {
            var startDate = options.StartDate.ToUniversalTime();

            if (startDate > DateTime.UtcNow && options.ReadNew)
            {
                throw new ArgumentException($"Invalid Start Date ({startDate})! Must be less or equal than UTC ({DateTime.UtcNow})!");
            }

            List<TopicPartitionOffset> startPartitionOffsets;

            using (var consumer = consumerFactory.CreateConsumer())
            {
                var partitionIds = GetTopicPartitionMetadata(options.TopicName, options.TimeoutInMilliseconds).Select(p => p.PartitionId);

                startPartitionOffsets = consumer.OffsetsForTimes(
                    partitionIds.Select(p => new TopicPartitionTimestamp(new TopicPartition(options.TopicName, p), new Timestamp(startDate))),
                    TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds));
            }

            var partitionTasks = startPartitionOffsets.Select(offset =>
                Task.Run(() => GetSomePartitionMessagesStartFromDateAsync(offset, options, cancellationToken), cancellationToken));

            var partitionMessages = await Task.WhenAll(partitionTasks).ConfigureAwait(false);
            var result = partitionMessages.SelectMany(x => x);

            if (options.ReadNew)
            {
                result = result.OrderBy(x => x.CreationTimeUTC);
            }
            else
            {
                result = result.OrderByDescending(x => x.CreationTimeUTC);
            }

            return result.Take(options.NumberOfMessages);
        }

        private List<ConsumeResponse> GetSomePartitionMessagesStartFromDateAsync(
            TopicPartitionOffset startPartitionOffset,
            GetSomeMessagesFromDateOptions options,
            CancellationToken cancellationToken)
        {
            var partitionMessages = new List<ConsumeResponse>();
            int readMessageAmount = 0;

            var (startOffset, endOffset, adjustedStartOffset) = AdjustStartOffset(startPartitionOffset, options);
            var currentPartitionOffset = adjustedStartOffset;

            while (!cancellationToken.IsCancellationRequested && readMessageAmount < options.NumberOfMessages)
            {
                var consumeResult = ConsumeMessage(currentPartitionOffset, options);
                if (consumeResult?.Message?.Value == null)
                {
                    break;
                }

                if (IsMessageWithinDateRange(consumeResult.Message.Timestamp.UtcDateTime, options))
                {
                    partitionMessages.Add(new ConsumeResponse(consumeResult.Message.Value, consumeResult.Message.Timestamp.UtcDateTime));
                    readMessageAmount++;
                }

                currentPartitionOffset = GetNextOffset(consumeResult, options.ReadNew, startOffset, endOffset);
                if (currentPartitionOffset == null)
                {
                    break;
                }
            }

            return partitionMessages;
        }

        private (Offset startOffset, Offset endOffset, TopicPartitionOffset adjustedStartOffset) AdjustStartOffset(
            TopicPartitionOffset startPartitionOffset, GetSomeMessagesFromDateOptions options)
        {
            using var consumer = consumerFactory.CreateConsumer();
            var watermarkOffsets = consumer.QueryWatermarkOffsets(
                startPartitionOffset.TopicPartition, TimeSpan.FromMilliseconds(options.TimeoutInMilliseconds));

            var startOffset = watermarkOffsets.Low;
            var endOffset = watermarkOffsets.High;

            TopicPartitionOffset adjustedStartOffset;

            if (startPartitionOffset.Offset == Offset.End || startPartitionOffset.Offset > endOffset)
            {
                adjustedStartOffset = new TopicPartitionOffset(startPartitionOffset.TopicPartition, endOffset - 1);
            }
            else if (startPartitionOffset.Offset < startOffset)
            {
                adjustedStartOffset = new TopicPartitionOffset(startPartitionOffset.TopicPartition, startOffset);
            }
            else
            {
                adjustedStartOffset = startPartitionOffset;
            }

            return (startOffset, endOffset, adjustedStartOffset);
        }

        #endregion

        #region Private Helpers

        private async Task WaitForTopicAsync(string topicName, int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var metadata = adminClient.GetMetadata(topicName, TimeSpan.FromMilliseconds(timeoutInMilliseconds));

                if (metadata.Topics.Any(t => t.Error == ErrorCode.NoError || t.Error == null))
                {
                    return;
                }

                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            }
        }

        private ConsumeResult<string, string>? ConsumeMessage(TopicPartitionOffset partitionOffset, GetSomeMessagesFromDateOptions options)
        {
            using var consumer = consumerFactory.CreateConsumer();
            consumer.Assign(partitionOffset);
            return consumer.Consume(options.TimeoutInMilliseconds);
        }

        private async Task<Offset> AdjustOffsetAsync(string topic, int timeoutInMilliseconds, Offset consumeFrom, CancellationToken cancellationToken)
        {
            if (consumeFrom == Offset.Stored)
            {
                var topicMessageAmount = await GetTopicMessageAmountAsync(topic, timeoutInMilliseconds, cancellationToken).ConfigureAwait(false);
                if (topicMessageAmount == 0)
                {
                    consumeFrom = Offset.Beginning;
                }
            }

            return consumeFrom;
        }

        private static TopicPartitionOffset? GetNextOffset(ConsumeResult<string, string> consumeResult, bool readNew, Offset lowWatermark, Offset highWatermark)
        {
            var currentOffset = consumeResult.Offset;
            if (readNew)
            {
                if (currentOffset >= highWatermark)
                {
                    return null;
                }

                return new TopicPartitionOffset(consumeResult.TopicPartition, currentOffset + 1);
            }
            else
            {
                var nextOffset = new Offset(currentOffset - 1);
                if (nextOffset < lowWatermark)
                {
                    return null;
                }

                return new TopicPartitionOffset(consumeResult.TopicPartition, nextOffset);
            }
        }

        private List<PartitionMetadata> GetTopicPartitionMetadata(string topicName, int timeoutInMilliseconds)
        {
            var metadata = adminClient.GetMetadata(topicName, TimeSpan.FromMilliseconds(timeoutInMilliseconds));
            return metadata.Topics.First(x => x.Topic == topicName).Partitions;
        }

        private static bool IsConsumeResultValid(ConsumeResult<string, string> consumeResult)
        {
            return
                consumeResult != null &&
                consumeResult.Message != null &&
                !consumeResult.IsPartitionEOF &&
                !string.IsNullOrEmpty(consumeResult.Message.Value);
        }

        private static bool IsNextPartitionOffSetValid(TopicPartitionOffset? nextPartitionOffset, Offset lastOffset)
        {
            return
                nextPartitionOffset != null &&
                nextPartitionOffset.Offset != Offset.End &&
                nextPartitionOffset.Offset != Offset.Unset &&
                nextPartitionOffset.Offset < lastOffset;
        }

        private static bool IsMessageWithinDateRange(DateTime messageTimestamp, GetSomeMessagesFromDateOptions options)
        {
            return (messageTimestamp >= options.StartDate && options.ReadNew) ||
                   (messageTimestamp <= options.StartDate && !options.ReadNew);
        }

        #endregion
    }
}