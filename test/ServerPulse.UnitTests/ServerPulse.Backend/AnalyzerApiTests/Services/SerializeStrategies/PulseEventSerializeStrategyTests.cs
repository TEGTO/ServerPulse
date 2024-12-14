using AnalyzerApi.Infrastructure.Models.Wrappers;
using AutoMapper;
using EventCommunication;
using MessageBus.Interfaces;
using Moq;
using System.Text.Json;

namespace AnalyzerApi.Services.SerializeStrategies.Tests
{
    [TestFixture]
    internal class PulseEventSerializeStrategyTests
    {
        private Mock<IMapper> mockMapper;
        private PulseEventSerializeStrategy serializeStrategy;

        [SetUp]
        public void Setup()
        {
            mockMapper = new Mock<IMapper>();

            serializeStrategy = new PulseEventSerializeStrategy(mockMapper.Object);
        }

        [Test]
        public void SerializeResponse_ValidConsumeResponse_ReturnsPulseEventWrapper()
        {
            // Arrange
            var pulseEvent = new PulseEvent("testKey", true);
            var serializedMessage = JsonSerializer.Serialize(pulseEvent);
            var consumeResponse = new ConsumeResponse(serializedMessage, DateTime.UtcNow);

            mockMapper.Setup(m => m.Map<PulseEventWrapper>(It.IsAny<PulseEvent>()))
                .Returns(new PulseEventWrapper { Id = pulseEvent.Id, Key = pulseEvent.Key, IsAlive = pulseEvent.IsAlive });

            // Act
            var result = serializeStrategy.SerializeResponse(consumeResponse);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(pulseEvent.Id));
            Assert.That(result.Key, Is.EqualTo(pulseEvent.Key));
            Assert.That(result.IsAlive, Is.EqualTo(pulseEvent.IsAlive));
            Assert.That(result.CreationDateUTC, Is.EqualTo(consumeResponse.CreationTimeUTC));
        }

        [Test]
        public void SerializeResponse_InvalidJson_ReturnsNull()
        {
            // Arrange
            var invalidJson = "invalid_json";
            var consumeResponse = new ConsumeResponse(invalidJson, DateTime.UtcNow);

            // Act
            var result = serializeStrategy.SerializeResponse(consumeResponse);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void SerializeResponse_NullEvent_ReturnsNull()
        {
            // Arrange
            var nullEventJson = JsonSerializer.Serialize<PulseEvent>(null!);
            var consumeResponse = new ConsumeResponse(nullEventJson, DateTime.UtcNow);

            // Act
            var result = serializeStrategy.SerializeResponse(consumeResponse);

            // Assert
            Assert.IsNull(result);
        }
    }
}