using AnalyzerApi;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Services.Receivers.Event;
using AutoMapper;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerPulse.EventCommunication.Events;
using System.Reflection;

namespace AnalyzerApiTests.Services.Receivers.Event
{
    [TestFixture]
    public class CustomEventReceiverTests
    {
        private const string CustomTopic = "custom-topic";
        private const string KafkaTimeoutInMilliseconds = "5000";

        private Mock<IMessageConsumer> mockMessageConsumer;
        private Mock<IMapper> mockMapper;
        private Mock<IConfiguration> mockConfiguration;
        private CustomEventReceiver customEventReceiver;
        private EventReceiverTopicData<CustomEventWrapper> topicData;

        [SetUp]
        public void Setup()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();
            mockMapper = new Mock<IMapper>();
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_CUSTOM_TOPIC])
                             .Returns(CustomTopic);
            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS])
                             .Returns(KafkaTimeoutInMilliseconds);

            topicData = new EventReceiverTopicData<CustomEventWrapper>("origin-topic");
            customEventReceiver = new CustomEventReceiver(mockMessageConsumer.Object, mockMapper.Object, mockConfiguration.Object, topicData);
        }

        [Test]
        public void ConvertToEventWrapper_DeserializationSucceeds_ReturnsEventWrapper()
        {
            // Arrange
            var consumeResponse = new ConsumeResponse(new CustomEvent("", "", "").ToString(), DateTime.UtcNow);
            var customEventWrapper = new CustomEventWrapper();
            mockMapper.Setup(m => m.Map<CustomEventWrapper>(It.IsAny<CustomEvent>()))
                      .Returns(customEventWrapper);
            // Act
            var result = InvokeProtectedConvertToEventWrapper(customEventReceiver, consumeResponse, mockMapper.Object);
            // Assert
            Assert.IsNotNull(result);
            Assert.That(result!.SerializedMessage, Is.EqualTo(consumeResponse.Message));
        }
        [Test]
        public void ConvertToEventWrapper_DeserializationFails_ReturnsNull()
        {
            // Arrange
            var consumeResponse = new ConsumeResponse("invalidMessage", DateTime.UtcNow);
            // Simulate deserialization failure by using invalid message content
            mockMapper.Setup(m => m.Map<CustomEventWrapper>(It.IsAny<CustomEvent>()))
                      .Returns((CustomEventWrapper)null);
            // Act
            var result = InvokeProtectedConvertToEventWrapper(customEventReceiver, consumeResponse, mockMapper.Object);
            // Assert
            Assert.IsNull(result);
        }

        private CustomEventWrapper? InvokeProtectedConvertToEventWrapper(CustomEventReceiver receiver, ConsumeResponse response, IMapper mapper)
        {
            var methodInfo = receiver.GetType()
                                     .BaseType // Get the base class (EventReceiver)
                                     .GetMethod("ConvertToEventWrapper", BindingFlags.NonPublic | BindingFlags.Instance);
            return (CustomEventWrapper?)methodInfo?.Invoke(receiver, new object[] { response, mapper });
        }
    }
}