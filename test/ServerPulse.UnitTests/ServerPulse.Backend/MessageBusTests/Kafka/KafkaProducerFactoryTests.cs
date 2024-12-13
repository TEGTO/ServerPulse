﻿using Confluent.Kafka;
using MessageBus.Kafka;
using Moq;

namespace MessageBusTests.Kafka
{
    [TestFixture]
    internal class KafkaProducerFactoryTests
    {
        private Mock<ProducerBuilder<string, string>> mockProducerBuilder;
        private ProducerConfig config;
        private KafkaProducerFactory factory;

        [SetUp]
        public void Setup()
        {
            config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092"
            };
            mockProducerBuilder = new Mock<ProducerBuilder<string, string>>(config);
            var mockProducer = new Mock<IProducer<string, string>>();
            mockProducerBuilder
                .Setup(pb => pb.Build())
                .Returns(mockProducer.Object);
            factory = new KafkaProducerFactory(config);
        }

        [Test]
        public void CreateProducer_ReturnsProducer()
        {
            // Act
            var producer = factory.CreateProducer();
            // Assert
            Assert.IsNotNull(producer);
            Assert.IsInstanceOf<IProducer<string, string>>(producer);
        }
    }
}