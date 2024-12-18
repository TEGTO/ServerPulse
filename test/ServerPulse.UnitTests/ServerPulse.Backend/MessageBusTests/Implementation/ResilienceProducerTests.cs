using Confluent.Kafka;
using Moq;
using Polly;

namespace MessageBus.Implementation.Tests
{
    [TestFixture]
    internal class ResilienceProducerTests
    {
        private Mock<IProducer<string, string>> mockProducer;
        private ResilienceProducer resilienceProducer;

        [SetUp]
        public void SetUp()
        {
            mockProducer = new Mock<IProducer<string, string>>();

            resilienceProducer = new ResilienceProducer(mockProducer.Object, ResiliencePipeline.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            resilienceProducer.Dispose();
        }

        [Test]
        public void Produce_CallsUnderlyingProducer()
        {
            // Arrange
            var topic = "test-topic";
            var message = new Message<string, string> { Key = "key", Value = "value" };
            Action<DeliveryReport<string, string>> deliveryHandler = _ => { };

            // Act
            resilienceProducer.Produce(topic, message, deliveryHandler);

            // Assert
            mockProducer.Verify(p => p.Produce(topic, message, deliveryHandler), Times.Once);
        }

        [Test]
        public async Task ProduceAsync_CallsUnderlyingProducer()
        {
            // Arrange
            var topic = "test-topic";
            var message = new Message<string, string> { Key = "key", Value = "value" };

            // Act
            await resilienceProducer.ProduceAsync(topic, message);

            // Assert
            mockProducer.Verify(p => p.ProduceAsync(topic, message, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Flush_CallsUnderlyingProducer()
        {
            // Arrange
            var timeout = TimeSpan.FromSeconds(1);

            // Act
            resilienceProducer.Flush(timeout);

            // Assert
            mockProducer.Verify(p => p.Flush(timeout), Times.Once);
        }

        [Test]
        public void Dispose_CallsUnderlyingProducer()
        {
            // Act
            resilienceProducer.Dispose();

            // Assert
            mockProducer.Verify(p => p.Dispose(), Times.Once);
        }

        [Test]
        public void BeginTransaction_CallsUnderlyingProducer()
        {
            // Act
            resilienceProducer.BeginTransaction();

            // Assert
            mockProducer.Verify(p => p.BeginTransaction(), Times.Once);
        }

        [Test]
        public void CommitTransaction_CallsUnderlyingProducer()
        {
            // Act
            resilienceProducer.CommitTransaction();

            // Assert
            mockProducer.Verify(p => p.CommitTransaction(), Times.Once);
        }
    }
}