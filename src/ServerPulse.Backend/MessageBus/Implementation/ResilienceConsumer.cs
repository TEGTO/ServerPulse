using Confluent.Kafka;
using Polly;

namespace MessageBus.Implementation
{
    public sealed class ResilienceConsumer : IConsumer<string, string>
    {
        private readonly IConsumer<string, string> consumer;
        private readonly ResiliencePipeline resiliencePipeline;

        public string MemberId => consumer.MemberId;
        public List<TopicPartition> Assignment => consumer.Assignment;
        public List<string> Subscription => consumer.Subscription;
        public IConsumerGroupMetadata ConsumerGroupMetadata => consumer.ConsumerGroupMetadata;
        public Handle Handle => consumer.Handle;
        public string Name => consumer.Name;

        public ResilienceConsumer(IConsumer<string, string> consumer, ResiliencePipeline resiliencePipeline)
        {
            this.consumer = consumer;
            this.resiliencePipeline = resiliencePipeline;
        }

        public int AddBrokers(string brokers)
        {
            int result = default;

            resiliencePipeline.Execute(() =>
            {
                result = consumer.AddBrokers(brokers);
            });

            return result;
        }

        public void Assign(TopicPartition partition)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Assign(partition);
            });
        }

        public void Assign(TopicPartitionOffset partition)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Assign(partition);
            });
        }

        public void Assign(IEnumerable<TopicPartitionOffset> partitions)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Assign(partitions);
            });
        }

        public void Assign(IEnumerable<TopicPartition> partitions)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Assign(partitions);
            });
        }

        public void Close()
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Close();
            });
        }

        public List<TopicPartitionOffset> Commit()
        {
            var result = new List<TopicPartitionOffset>();

            resiliencePipeline.Execute(() =>
            {
                result = consumer.Commit();
            });

            return result;
        }

        public void Commit(IEnumerable<TopicPartitionOffset> offsets)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Commit(offsets);
            });
        }

        public void Commit(ConsumeResult<string, string> result)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Commit(result);
            });
        }

        public List<TopicPartitionOffset> Committed(TimeSpan timeout)
        {
            var result = new List<TopicPartitionOffset>();

            resiliencePipeline.Execute(() =>
            {
                result = consumer.Committed(timeout);
            });

            return result;
        }

        public List<TopicPartitionOffset> Committed(IEnumerable<TopicPartition> partitions, TimeSpan timeout)
        {
            var result = new List<TopicPartitionOffset>();

            resiliencePipeline.Execute(() =>
            {
                result = consumer.Committed(partitions, timeout);
            });

            return result;
        }

        public ConsumeResult<string, string> Consume(int millisecondsTimeout)
        {
            var result = new ConsumeResult<string, string>();

            resiliencePipeline.Execute(() =>
            {
                result = consumer.Consume(millisecondsTimeout);
            });

            return result;
        }

        public ConsumeResult<string, string> Consume(CancellationToken cancellationToken = default)
        {
            var result = new ConsumeResult<string, string>();

            resiliencePipeline.Execute(() =>
            {
                result = consumer.Consume(cancellationToken);
            });

            return result;
        }

        public ConsumeResult<string, string> Consume(TimeSpan timeout)
        {
            var result = new ConsumeResult<string, string>();

            resiliencePipeline.Execute(() =>
            {
                result = consumer.Consume(timeout);
            });

            return result;
        }

        public void Dispose()
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Dispose();
            });
        }

        public WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition)
        {
            var result = new WatermarkOffsets(Offset.Unset, Offset.Unset);

            resiliencePipeline.Execute(() =>
            {
                result = consumer.GetWatermarkOffsets(topicPartition);
            });

            return result;
        }

        public void IncrementalAssign(IEnumerable<TopicPartitionOffset> partitions)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.IncrementalAssign(partitions);
            });
        }

        public void IncrementalAssign(IEnumerable<TopicPartition> partitions)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.IncrementalAssign(partitions);
            });
        }

        public void IncrementalUnassign(IEnumerable<TopicPartition> partitions)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.IncrementalUnassign(partitions);
            });
        }

        public List<TopicPartitionOffset> OffsetsForTimes(IEnumerable<TopicPartitionTimestamp> timestampsToSearch, TimeSpan timeout)
        {
            var result = new List<TopicPartitionOffset>();

            resiliencePipeline.Execute(() =>
            {
                result = consumer.OffsetsForTimes(timestampsToSearch, timeout);
            });

            return result;
        }

        public void Pause(IEnumerable<TopicPartition> partitions)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Pause(partitions);
            });
        }

        public Offset Position(TopicPartition partition)
        {
            Offset result = default;

            resiliencePipeline.Execute(() =>
            {
                result = consumer.Position(partition);
            });

            return result;
        }

        public WatermarkOffsets QueryWatermarkOffsets(TopicPartition topicPartition, TimeSpan timeout)
        {
            var result = new WatermarkOffsets(Offset.Unset, Offset.Unset);

            resiliencePipeline.Execute(() =>
            {
                result = consumer.QueryWatermarkOffsets(topicPartition, timeout);
            });

            return result;
        }

        public void Resume(IEnumerable<TopicPartition> partitions)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Resume(partitions);
            });
        }

        public void Seek(TopicPartitionOffset tpo)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Seek(tpo);
            });
        }

        public void SetSaslCredentials(string username, string password)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.SetSaslCredentials(username, password);
            });
        }

        public void StoreOffset(ConsumeResult<string, string> result)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.StoreOffset(result);
            });
        }

        public void StoreOffset(TopicPartitionOffset offset)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.StoreOffset(offset);
            });
        }

        public void Subscribe(IEnumerable<string> topics)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Subscribe(topics);
            });
        }

        public void Subscribe(string topic)
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Subscribe(topic);
            });
        }

        public void Unassign()
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Unassign();
            });
        }

        public void Unsubscribe()
        {
            resiliencePipeline.Execute(() =>
            {
                consumer.Unsubscribe();
            });
        }
    }
}