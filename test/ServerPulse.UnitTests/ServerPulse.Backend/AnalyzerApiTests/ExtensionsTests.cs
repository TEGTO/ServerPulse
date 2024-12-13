using AnalyzerApi.Infrastructure.Models.Wrappers;
using AutoMapper;
using EventCommunication.Events;
using MessageBus.Interfaces;
using Moq;
using System.Text.Json;

namespace AnalyzerApi.Tests
{
    [TestFixture]
    internal class ExtensionsTests
    {
        private Mock<IMapper> mockMapper;

        [SetUp]
        public void Setup()
        {
            mockMapper = new Mock<IMapper>();
        }

        [Test]
        public void TryDeserializeEventWrapper_ValidJson_ReturnsTrueAndValidWrapper()
        {
            // Arrange
            var ev = new TestEvent("testKey");
            var serializedMessage = JsonSerializer.Serialize(ev);
            var consumeResponse = new ConsumeResponse(serializedMessage, DateTime.UtcNow);

            mockMapper.Setup(m => m.Map<TestEventWrapper>(It.IsAny<TestEvent>()))
                .Returns(new TestEventWrapper { Id = ev.Id, Key = ev.Key });

            // Act
            var result = consumeResponse.TryDeserializeEventWrapper<TestEvent, TestEventWrapper>(mockMapper.Object, out var wrapper);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(wrapper);
            Assert.That(wrapper.Id, Is.EqualTo(ev.Id));
            Assert.That(wrapper.Key, Is.EqualTo(ev.Key));
            Assert.That(wrapper.CreationDateUTC, Is.EqualTo(consumeResponse.CreationTimeUTC));
        }

        [Test]
        public void TryDeserializeEventWrapper_InvalidJson_ReturnsFalseAndNullWrapper()
        {
            // Arrange
            var invalidJson = "invalid_json";
            var consumeResponse = new ConsumeResponse(invalidJson, DateTime.UtcNow);

            // Act
            var result = consumeResponse.TryDeserializeEventWrapper<TestEvent, TestEventWrapper>(mockMapper.Object, out var wrapper);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(wrapper);
        }

        [Test]
        public void TryDeserializeEventWrapper_NullEvent_ReturnsFalseAndNullWrapper()
        {
            // Arrange
            var nullEventJson = JsonSerializer.Serialize<TestEvent>(null!);
            var consumeResponse = new ConsumeResponse(nullEventJson, DateTime.UtcNow);

            // Act
            var result = consumeResponse.TryDeserializeEventWrapper<TestEvent, TestEventWrapper>(mockMapper.Object, out var wrapper);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(wrapper);
        }
    }

    public sealed record TestEvent(string Key) : BaseEvent(Key);
    public class TestEventWrapper : BaseEventWrapper { }
}