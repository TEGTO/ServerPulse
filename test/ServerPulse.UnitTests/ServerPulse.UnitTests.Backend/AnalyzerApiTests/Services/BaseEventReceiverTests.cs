using AnalyzerApi;
using AutoMapper;
using MessageBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AnalyzerApiTests.Services
{
    internal abstract class BaseEventReceiverTests
    {
        protected const int KAFKA_TIMEOUT_IN_MILLISECONDS = 5000;

        protected Mock<IMessageConsumer> mockMessageConsumer;
        protected Mock<IMapper> mockMapper;
        protected Mock<IConfiguration> mockConfiguration;

        [SetUp]
        public virtual void Setup()
        {
            mockMessageConsumer = new Mock<IMessageConsumer>();
            mockMapper = new Mock<IMapper>();
            mockConfiguration = new Mock<IConfiguration>();

            mockConfiguration.SetupGet(c => c[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]).Returns(KAFKA_TIMEOUT_IN_MILLISECONDS.ToString());

        }
    }
}