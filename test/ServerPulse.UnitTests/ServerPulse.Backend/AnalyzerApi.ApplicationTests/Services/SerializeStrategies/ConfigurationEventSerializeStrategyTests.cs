using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using EventCommunication;
using MessageBus.Models;
using Moq;
using System.Text.Json;

namespace AnalyzerApi.Application.Services.SerializeStrategies.Tests
{
    [TestFixture]
    internal class ConfigurationEventSerializeStrategyTests
    {
        private Mock<IMapper> mockMapper;
        private ConfigurationEventSerializeStrategy serializeStrategy;

        [SetUp]
        public void Setup()
        {
            mockMapper = new Mock<IMapper>();
            serializeStrategy = new ConfigurationEventSerializeStrategy(mockMapper.Object);
        }

        [Test]
        public void SerializeResponse_ValidConsumeResponse_ReturnsConfigurationEventWrapper()
        {
            // Arrange
            var configurationEvent = new ConfigurationEvent("testKey", TimeSpan.FromSeconds(30));
            var serializedMessage = JsonSerializer.Serialize(configurationEvent);
            var consumeResponse = new ConsumeResponse(serializedMessage, DateTime.UtcNow);

            var expectedWrapper = new ConfigurationEventWrapper
            {
                Id = configurationEvent.Id,
                Key = configurationEvent.Key,
                ServerKeepAliveInterval = configurationEvent.ServerKeepAliveInterval,
                CreationDateUTC = consumeResponse.CreationTimeUTC
            };

            mockMapper.Setup(m => m.Map<ConfigurationEventWrapper>(It.IsAny<ConfigurationEvent>()))
                      .Returns(expectedWrapper);

            // Act
            var result = serializeStrategy.SerializeResponse(consumeResponse);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(configurationEvent.Id));
            Assert.That(result.Key, Is.EqualTo(configurationEvent.Key));
            Assert.That(result.ServerKeepAliveInterval, Is.EqualTo(configurationEvent.ServerKeepAliveInterval));
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
            var nullEventJson = JsonSerializer.Serialize<ConfigurationEvent>(null!);
            var consumeResponse = new ConsumeResponse(nullEventJson, DateTime.UtcNow);

            // Act
            var result = serializeStrategy.SerializeResponse(consumeResponse);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void SerializeResponse_ValidResponseWithCustomKeepAliveInterval()
        {
            // Arrange
            var configurationEvent = new ConfigurationEvent("uniqueKey", TimeSpan.FromMinutes(10));
            var serializedMessage = JsonSerializer.Serialize(configurationEvent);
            var consumeResponse = new ConsumeResponse(serializedMessage, DateTime.UtcNow);

            var expectedWrapper = new ConfigurationEventWrapper
            {
                Id = configurationEvent.Id,
                Key = configurationEvent.Key,
                ServerKeepAliveInterval = configurationEvent.ServerKeepAliveInterval,
                CreationDateUTC = consumeResponse.CreationTimeUTC
            };

            mockMapper.Setup(m => m.Map<ConfigurationEventWrapper>(It.IsAny<ConfigurationEvent>()))
                      .Returns(expectedWrapper);

            // Act
            var result = serializeStrategy.SerializeResponse(consumeResponse);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ServerKeepAliveInterval, Is.EqualTo(TimeSpan.FromMinutes(10)));
        }
    }
}