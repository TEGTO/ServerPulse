using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using EventCommunication;
using MessageBus.Models;
using Moq;
using System.Text.Json;

namespace AnalyzerApi.Application.Services.SerializeStrategies.Tests
{
    [TestFixture]
    internal class CustomEventSerializeStrategyTests
    {
        private Mock<IMapper> mockMapper;
        private CustomEventSerializeStrategy serializeStrategy;

        [SetUp]
        public void Setup()
        {
            mockMapper = new Mock<IMapper>();
            serializeStrategy = new CustomEventSerializeStrategy(mockMapper.Object);
        }

        [Test]
        public void SerializeResponse_ValidConsumeResponse_ReturnsCustomEventWrapper()
        {
            // Arrange
            var customEvent = new CustomEvent("testKey", "TestName", "TestDescription");
            var serializedMessage = JsonSerializer.Serialize(customEvent);
            var consumeResponse = new ConsumeResponse(serializedMessage, DateTime.UtcNow);

            var expectedWrapper = new CustomEventWrapper
            {
                Id = customEvent.Id,
                Key = customEvent.Key,
                Name = customEvent.Name,
                Description = customEvent.Description,
                SerializedMessage = serializedMessage,
                CreationDateUTC = consumeResponse.CreationTimeUTC
            };

            mockMapper.Setup(m => m.Map<CustomEventWrapper>(It.IsAny<CustomEvent>())).Returns(expectedWrapper);

            // Act
            var result = serializeStrategy.SerializeResponse(consumeResponse);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(customEvent.Id));
            Assert.That(result.Key, Is.EqualTo(customEvent.Key));
            Assert.That(result.Name, Is.EqualTo(customEvent.Name));
            Assert.That(result.Description, Is.EqualTo(customEvent.Description));
            Assert.That(result.SerializedMessage, Is.EqualTo(serializedMessage));
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
            var nullEventJson = JsonSerializer.Serialize<CustomEvent>(null!);
            var consumeResponse = new ConsumeResponse(nullEventJson, DateTime.UtcNow);

            // Act
            var result = serializeStrategy.SerializeResponse(consumeResponse);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void SerializeResponse_ValidResponse_WithCustomProperties()
        {
            // Arrange
            var customEvent = new CustomEvent("uniqueKey", "SampleName", "SampleDescription");
            var serializedMessage = JsonSerializer.Serialize(customEvent);
            var consumeResponse = new ConsumeResponse(serializedMessage, DateTime.UtcNow);

            var expectedWrapper = new CustomEventWrapper
            {
                Id = customEvent.Id,
                Key = customEvent.Key,
                Name = customEvent.Name,
                Description = customEvent.Description,
                SerializedMessage = serializedMessage,
                CreationDateUTC = consumeResponse.CreationTimeUTC
            };

            mockMapper.Setup(m => m.Map<CustomEventWrapper>(It.IsAny<CustomEvent>())).Returns(expectedWrapper);

            // Act
            var result = serializeStrategy.SerializeResponse(consumeResponse);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.SerializedMessage, Is.EqualTo(serializedMessage));
        }
    }
}