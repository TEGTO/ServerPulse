using Confluent.Kafka;
using Polly;

namespace MessageBus.Implementation
{
    public sealed class ResilienceProducer : IProducer<string, string>
    {
        private readonly IProducer<string, string> producer;
        private readonly ResiliencePipeline resiliencePipeline;

        public Handle Handle => producer.Handle;
        public string Name => producer.Name;

        public ResilienceProducer(IProducer<string, string> producer, ResiliencePipeline resiliencePipeline)
        {
            this.producer = producer;
            this.resiliencePipeline = resiliencePipeline;
        }

        public void AbortTransaction(TimeSpan timeout)
        {
            resiliencePipeline.Execute(() =>
            {
                producer.AbortTransaction(timeout);
            });
        }

        public void AbortTransaction()
        {
            resiliencePipeline.Execute(() =>
            {
                producer.AbortTransaction();
            });
        }

        public int AddBrokers(string brokers)
        {
            int result = default;

            resiliencePipeline.Execute(() =>
            {
                result = producer.AddBrokers(brokers);
            });

            return result;
        }

        public void BeginTransaction()
        {
            resiliencePipeline.Execute(() =>
            {
                producer.BeginTransaction();
            });
        }

        public void CommitTransaction(TimeSpan timeout)
        {
            resiliencePipeline.Execute(() =>
            {
                producer.CommitTransaction(timeout);
            });
        }

        public void CommitTransaction()
        {
            resiliencePipeline.Execute(() =>
            {
                producer.CommitTransaction();
            });
        }

        public void Dispose()
        {
            resiliencePipeline.Execute(() =>
            {
                producer.Dispose();
            });
        }

        public int Flush(TimeSpan timeout)
        {
            int result = default;

            resiliencePipeline.Execute(() =>
            {
                result = producer.Flush(timeout);
            });

            return result;
        }

        public void Flush(CancellationToken cancellationToken = default)
        {
            resiliencePipeline.Execute(() =>
            {
                producer.Flush(cancellationToken);
            });
        }

        public void InitTransactions(TimeSpan timeout)
        {
            resiliencePipeline.Execute(() =>
            {
                producer.InitTransactions(timeout);
            });
        }

        public int Poll(TimeSpan timeout)
        {
            int result = default;

            resiliencePipeline.Execute(() =>
            {
                result = producer.Poll(timeout);
            });

            return result;
        }

        public void Produce(string topic, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryHandler = null)
        {
            resiliencePipeline.Execute(() =>
            {
                producer.Produce(topic, message, deliveryHandler);
            });
        }

        public void Produce(TopicPartition topicPartition, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryHandler = null)
        {
            resiliencePipeline.Execute(() =>
            {
                producer.Produce(topicPartition, message, deliveryHandler);
            });
        }

        public async Task<DeliveryResult<string, string>> ProduceAsync(string topic, Message<string, string> message, CancellationToken cancellationToken = default)
        {
            DeliveryResult<string, string>? deliveryResult = null;

            await resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                deliveryResult = await producer.ProduceAsync(topic, message, ct).ConfigureAwait(false);
            }, cancellationToken: cancellationToken).ConfigureAwait(false);

            return deliveryResult == null ? new DeliveryResult<string, string>() : deliveryResult;
        }

        public async Task<DeliveryResult<string, string>> ProduceAsync(TopicPartition topicPartition, Message<string, string> message, CancellationToken cancellationToken = default)
        {
            DeliveryResult<string, string>? deliveryResult = null;

            await resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                deliveryResult = await producer.ProduceAsync(topicPartition, message, cancellationToken).ConfigureAwait(false);
            }, cancellationToken: cancellationToken).ConfigureAwait(false);

            return deliveryResult == null ? new DeliveryResult<string, string>() : deliveryResult;
        }

        public void SendOffsetsToTransaction(IEnumerable<TopicPartitionOffset> offsets, IConsumerGroupMetadata groupMetadata, TimeSpan timeout)
        {
            resiliencePipeline.Execute(() =>
            {
                producer.SendOffsetsToTransaction(offsets, groupMetadata, timeout);
            });
        }

        public void SetSaslCredentials(string username, string password)
        {
            resiliencePipeline.Execute(() =>
            {
                producer.SetSaslCredentials(username, password);
            });
        }
    }
}
