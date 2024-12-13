using AnalyzerApi.Infrastructure.Models.Wrappers;
using AutoMapper;
using EventCommunication.Events;
using MessageBus.Interfaces;
using Moq;
using System.Text.Json;

namespace AnalyzerApi.Services.SerializeStrategies.Tests
{
    [TestFixture]
    internal class LoadEventSerializeStrategyTests
    {
        private Mock<IMapper> mockMapper;
        private LoadEventSerializeStrategy serializeStrategy;

        [SetUp]
        public void Setup()
        {
            mockMapper = new Mock<IMapper>();
            serializeStrategy = new LoadEventSerializeStrategy(mockMapper.Object);
        }

        [Test]
        public void SerializeResponse_ValidConsumeResponse_ReturnsLoadEventWrapper()
        {
            // Arrange
            var loadEvent = new LoadEvent("testKey", "/api/test", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow);
            var serializedMessage = JsonSerializer.Serialize(loadEvent);
            var consumeResponse = new ConsumeResponse(serializedMessage, DateTime.UtcNow);

            var expectedWrapper = new LoadEventWrapper
            {
                Id = loadEvent.Id,
                Key = loadEvent.Key,
                Endpoint = loadEvent.Endpoint,
                Method = loadEvent.Method,
                StatusCode = loadEvent.StatusCode,
                Duration = loadEvent.Duration,
                TimestampUTC = loadEvent.TimestampUTC,
                CreationDateUTC = consumeResponse.CreationTimeUTC
            };

            mockMapper.Setup(m => m.Map<LoadEventWrapper>(It.IsAny<LoadEvent>())).Returns(expectedWrapper);

            // Act
            var result = serializeStrategy.SerializeResponse(consumeResponse);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(loadEvent.Id));
            Assert.That(result.Key, Is.EqualTo(loadEvent.Key));
            Assert.That(result.Endpoint, Is.EqualTo(loadEvent.Endpoint));
            Assert.That(result.Method, Is.EqualTo(loadEvent.Method));
            Assert.That(result.StatusCode, Is.EqualTo(loadEvent.StatusCode));
            Assert.That(result.Duration, Is.EqualTo(loadEvent.Duration));
            Assert.That(result.TimestampUTC, Is.EqualTo(loadEvent.TimestampUTC));
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
            var nullEventJson = JsonSerializer.Serialize<LoadEvent>(null!);
            var consumeResponse = new ConsumeResponse(nullEventJson, DateTime.UtcNow);

            // Act
            var result = serializeStrategy.SerializeResponse(consumeResponse);

            // Assert
            Assert.IsNull(result);
        }
    }
}